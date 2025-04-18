using DsohScraper;
using DsohScraper.Services;

// Display welcome message
ConsoleService.LogInfo("Welcome to the DSOH Scraper!");

// Cross-platform default: user's Downloads folder
var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
var defaultOutputDirectory = Path.Combine(homeDir, "Downloads");
var outputDirectory = ConsoleService.PromptForDownloadDirectory(defaultOutputDirectory);

const int defaultMaxParallelDownloads = 3;
var maxParallelDownloads = ConsoleService.PromptForMaxParallelDownloads(defaultMaxParallelDownloads);

// Pass user-supplied directory to AudioDownloader
var downloader = new DownloadService(outputDirectory, maxParallelDownloads);

// Get show numbers to download
var showNumbers = new List<int>();

if (args.Length > 0)
{
    // Parse show numbers from command line arguments
    foreach (var arg in args)
    {
        if (int.TryParse(arg, out var showNumber))
        {
            showNumbers.Add(showNumber);
        }
        else if (ConsoleService.TryParseRange(arg, out var range))
        {
            showNumbers.AddRange(range);
        }
    }
}
else
{
    // Prompt user for show numbers
    showNumbers = ConsoleService.PromptForShowNumbers();
}

if (showNumbers.Count == 0)
{
    ConsoleService.LogError("No valid show numbers provided.");
    return 1;
}

// Confirm download
if (!ConsoleService.ConfirmDownload(showNumbers))
{
    ConsoleService.LogInfo("Download cancelled.");
    return 0;
}

// Download the shows
try
{
    // Initialize SoundCloud client
    await downloader.InitializeSoundCloudClientAsync();

    await ProgressService.Instance.RunAsync(() => downloader.DownloadShowsAsync(showNumbers));

    return 0;
}
catch (Exception ex)
{
    ConsoleService.LogError($"\nError: {ex.Message}");
    return 1;
}