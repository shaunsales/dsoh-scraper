using DsohScraper;
using DsohScraper.Helpers;
using DsohScraper.Services;

// Display welcome message
ConsoleService.LogInfo("Welcome to the DSOH Scraper!");

static int PrintUsage()
{
    ConsoleService.LogInfo("Usage:");
    ConsoleService.LogInfo("  dsoh-scraper --output <directory> --shows <numbers|ranges> [--userId <id>] [--overwrite|--skip-existing]");
    ConsoleService.LogInfo("Examples:");
    ConsoleService.LogInfo("  dsoh-scraper --output ~/Downloads --shows 101,102,103-110");
    ConsoleService.LogInfo("  dsoh-scraper --output ./out --shows 1500 --overwrite");
    return 1;
}

if (!CliHelper.TryParse(args, out var cli, out var errorMessage, out var showUsage))
{
    if (!string.IsNullOrWhiteSpace(errorMessage))
        ConsoleService.LogError(errorMessage);

    if (showUsage)
        return PrintUsage();

    return 1;
}

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

// Pass user-supplied directory to AudioDownloader
const int userId = 2497;
var downloader = new DownloadService(cli.OutputDirectory, userId, cli.OverwriteExisting);

// Download the shows
try
{
    // Initialize SoundCloud client
    await downloader.InitializeSoundCloudClientAsync();

    await ProgressService.Instance.RunAsync(() => downloader.DownloadShowsAsync(cli.ShowNumbers, cts.Token));

    return 0;
}
catch (Exception ex)
{
    ConsoleService.LogError($"\nError: {ex.Message}");
    return 1;
}