using DsohScraper.Helpers;
using SoundCloudExplode;
using SoundCloudExplode.Common;
using SoundCloudExplode.Search;
using SoundCloudExplode.Tracks;

namespace DsohScraper.Services;

public class DownloadService(string outputDirectory, int userId, bool overwriteExisting)
{
    private readonly SoundCloudClient _soundCloudClient = new();
    private readonly int _userId = userId;
    private readonly bool _overwriteExisting = overwriteExisting;

    public async Task InitializeSoundCloudClientAsync()
    {
        try
        {
            await _soundCloudClient.InitializeAsync();
            ConsoleService.LogClientInitialized(_soundCloudClient.ClientId);
        }
        catch (Exception ex)
        {
            ConsoleService.LogClientInitializationError(ex.Message);
        }
    }

    public async Task DownloadShowsAsync(IEnumerable<int> showNumbers, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(showNumbers);
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        foreach (var showNumber in showNumbers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ProcessShowDownloadAsync(showNumber, cancellationToken);
        }
    }

    private async Task ProcessShowDownloadAsync(int showNumber, CancellationToken token)
    {
        var searchQuery = $"DSOH #{showNumber}";
        var filteredResults = await GetFilteredTracksAsync(
            searchQuery,
            _userId,
            searchQuery,
            token);

        if (filteredResults.Count == 0)
        {
            filteredResults = await GetFilteredTracksAsync(
                $"Deeper Shades of House #{showNumber}",
                _userId,
                $"#{showNumber}",
                token);
        }

        await DownloadShowAsync(showNumber, filteredResults, token);
    }

    private async Task DownloadShowAsync(int showNumber, List<TrackSearchResult> tracks, CancellationToken cancellationToken)
    {
        try
        {
            if (tracks.Count == 0)
            {
                ConsoleService.LogTrackUrlExtractionFailed(showNumber);
                return;
            }

            // Sanitize track title for filename
            foreach (var track in tracks)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var filePath = FileNameHelper.BuildMp3Path(outputDirectory, showNumber, track.Title);
                
                await DownloadTrackAsync(track, filePath, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            ConsoleService.LogError($"Error downloading show #{showNumber}: {ex.Message}");
        }
    }

    private async Task<List<TrackSearchResult>> GetFilteredTracksAsync(string searchQuery, int userId, string textFilter, CancellationToken cancellationToken)
    {
        var results = new List<TrackSearchResult>();
        await foreach (var track in _soundCloudClient.Search.GetTracksAsync(searchQuery: searchQuery, cancellationToken: cancellationToken)
                           .WithCancellation(cancellationToken))
        {
            results.Add(track);
        }

        return results.Where(track => track is
            {
                UserId: var id,
                Title: not null
            } && id == userId && track.Title.Contains(textFilter, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private async Task DownloadTrackAsync(Track track, string outputPath, CancellationToken cancellationToken)
    {
        var displayName = track.Title ?? Path.GetFileName(outputPath);
        var taskKey = outputPath;

        try
        {
            // Ensure the directory exists (CreateDirectory is idempotent)
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);
            
            if (File.Exists(outputPath))
            {
                if (!_overwriteExisting)
                {
                    ProgressService.Instance.AddTask(taskKey, $"{displayName} (already exists)");
                    ProgressService.Instance.CompleteTask(taskKey);
                    return;
                }

                try
                {
                    File.Delete(outputPath);
                }
                catch (Exception ex)
                {
                    ProgressService.Instance.AddTask(taskKey, $"{displayName} (failed to overwrite)");
                    ProgressService.Instance.CompleteTask(taskKey);
                    ConsoleService.LogError($"Failed to overwrite existing file '{outputPath}': {ex.Message}");
                    return;
                }
            }
            
            ProgressService.Instance.AddTask(taskKey, displayName);
            
            var progress = new Progress<double>(p =>
            {
                ProgressService.Instance.UpdateProgress(taskKey, p);
            });

            await _soundCloudClient.DownloadAsync(
                track,
                outputPath,
                progress: progress,
                cancellationToken: cancellationToken
            );

            ProgressService.Instance.CompleteTask(taskKey);
        }
        catch (OperationCanceledException)
        {
            ConsoleService.LogInfo($"Download canceled for track: {track.Title}");
            throw;
        }
        catch (Exception ex)
        {
            ConsoleService.LogError($"Error downloading track '{displayName}' to '{outputPath}': {ex.Message}");
            ProgressService.Instance.CompleteTask(taskKey);
        }
    }
}