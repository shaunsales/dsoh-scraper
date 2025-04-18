using DsohScraper.Extensions;
using SoundCloudExplode;
using SoundCloudExplode.Common;
using SoundCloudExplode.Search;
using SoundCloudExplode.Tracks;

namespace DsohScraper.Services;

public class DownloadService(string outputDirectory, int maxParallelDownloads)
{
    private readonly SoundCloudClient _soundCloudClient = new();

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

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxParallelDownloads,
            CancellationToken = cancellationToken
        };
        
        await Parallel.ForEachAsync(showNumbers, parallelOptions, ProcessShowDownloadAsync);
    }

    private async ValueTask ProcessShowDownloadAsync(int showNumber, CancellationToken token)
    {
        const int userId = 2497;
        var searchQuery = $"DSOH #{showNumber}";
        var filteredResults = await GetFilteredTracksAsync(
            searchQuery,
            userId,
            searchQuery,
            token);

        if (filteredResults.Count == 0)
        {
            filteredResults = await GetFilteredTracksAsync(
                $"Deeper Shades of House #{showNumber}",
                userId,
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
                var kebabTitle = track.Title?.ToAsciiKebabCase() ?? $"dsoh-{showNumber}-deeper-shades-of-house";
                var filePath = Path.Combine(outputDirectory, $"{kebabTitle}.mp3");
                
                await DownloadTrackAsync(track, filePath, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            ConsoleService.LogError($"Error downloading show #{showNumber}: {ex.Message}");
        }
    }

    private async Task<List<TrackSearchResult>> GetFilteredTracksAsync(string searchQuery, int userId, string textFilter, CancellationToken cancellationToken)
    {
        var results = await _soundCloudClient.Search.GetTracksAsync(searchQuery: searchQuery, cancellationToken: cancellationToken);
        return results.Where(track => track is
        {
            UserId: var id,
            Title: not null
        } && id == userId && track.Title.Contains(textFilter, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private async Task DownloadTrackAsync(Track track, string outputPath, CancellationToken cancellationToken)
    {
        try
        {
            if (track.Title is null)
            {
                ConsoleService.LogError("Track title is null.");
                return;
            }
            
            // Ensure the directory exists (CreateDirectory is idempotent)
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);
            
            if (File.Exists(outputPath))
            {
                ProgressService.Instance.AddTask(track.Title);
                ProgressService.Instance.UpdateProgress(track.Title, 1);
                return;
            }
            
            ProgressService.Instance.AddTask(track.Title);
            
            var progress = new Progress<double>(p =>
            {
                if (track.Title != null)
                    ProgressService.Instance.UpdateProgress(track.Title, p);
            });

            await _soundCloudClient.DownloadAsync(
                track,
                outputPath,
                progress: progress,
                cancellationToken: cancellationToken
            );

            if (track.Title != null)
                ProgressService.Instance.CompleteTask(track.Title);
        }
        catch (OperationCanceledException)
        {
            ConsoleService.LogInfo($"Download canceled for track: {track.Title}");
            throw;
        }
        catch (Exception ex)
        {
            ConsoleService.LogError($"Error downloading track {track.Title}: {ex.Message}");
            throw;
        }
    }
}