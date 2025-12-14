using DsohScraper;
using DsohScraper.Services;

// Display welcome message
ConsoleService.LogInfo("Welcome to the DSOH Scraper!");

static int PrintUsage()
{
    ConsoleService.LogInfo("Usage:");
    ConsoleService.LogInfo("  dsoh-scraper --output <directory> --shows <numbers|ranges> [--userId <id>]");
    ConsoleService.LogInfo("Examples:");
    ConsoleService.LogInfo("  dsoh-scraper --output ~/Downloads --shows 101,102,103-110");
    ConsoleService.LogInfo("  dsoh-scraper --output ./out --shows 1500 --userId 2497");
    return 1;
}

static Dictionary<string, string> ParseArgs(string[] args)
{
    var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (var i = 0; i < args.Length; i++)
    {
        var arg = args[i];

        if (arg is "--help" or "-h")
        {
            options[arg] = string.Empty;
            continue;
        }

        if (!arg.StartsWith("--", StringComparison.Ordinal))
            continue;

        var eqIndex = arg.IndexOf('=');
        if (eqIndex > 0)
        {
            var key = arg[..eqIndex];
            var value = arg[(eqIndex + 1)..];
            options[key] = value;
            continue;
        }

        if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
        {
            options[arg] = args[i + 1];
            i++;
            continue;
        }

        options[arg] = string.Empty;
    }

    return options;
}

static string ExpandTildePath(string path)
{
    if (string.IsNullOrWhiteSpace(path))
        return path;

    if (path is "~")
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    if (path.StartsWith("~/", StringComparison.Ordinal))
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, path[2..]);
    }

    return path;
}

var options = ParseArgs(args);

if (args.Length == 0 || options.ContainsKey("--help") || options.ContainsKey("-h"))
    return PrintUsage();

var allowedOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "--output",
    "--shows",
    "--userId",
    "--help",
    "-h"
};

var unknownOptions = options.Keys.Where(k => !allowedOptions.Contains(k)).ToList();
if (unknownOptions.Count > 0)
{
    ConsoleService.LogError($"Unknown option(s): {string.Join(", ", unknownOptions)}");
    return PrintUsage();
}

if (!options.TryGetValue("--output", out var outputDirectory) || string.IsNullOrWhiteSpace(outputDirectory))
{
    ConsoleService.LogError("Missing required option: --output");
    return PrintUsage();
}

outputDirectory = ExpandTildePath(outputDirectory);

if (!options.TryGetValue("--shows", out var showsArg) || string.IsNullOrWhiteSpace(showsArg))
{
    ConsoleService.LogError("Missing required option: --shows");
    return PrintUsage();
}

const int defaultUserId = 2497;
var userId = defaultUserId;
if (options.TryGetValue("--userId", out var userIdArg) && !string.IsNullOrWhiteSpace(userIdArg))
{
    if (!int.TryParse(userIdArg, out userId))
    {
        ConsoleService.LogError("Invalid --userId value.");
        return PrintUsage();
    }
}

if (!ConsoleService.TryParseRange(showsArg, out var showNumbers) || showNumbers.Count == 0)
{
    ConsoleService.LogError("No valid show numbers provided in --shows.");
    return 1;
}

var orderedShowNumbers = showNumbers.Distinct().OrderBy(n => n).ToList();

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

// Pass user-supplied directory to AudioDownloader
var downloader = new DownloadService(outputDirectory, userId);

// Download the shows
try
{
    // Initialize SoundCloud client
    await downloader.InitializeSoundCloudClientAsync();

    await ProgressService.Instance.RunAsync(() => downloader.DownloadShowsAsync(orderedShowNumbers, cts.Token));

    return 0;
}
catch (Exception ex)
{
    ConsoleService.LogError($"\nError: {ex.Message}");
    return 1;
}