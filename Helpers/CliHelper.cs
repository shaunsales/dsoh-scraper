using DsohScraper.Services;

namespace DsohScraper.Helpers;

public sealed record CliOptions(
    string OutputDirectory,
    IReadOnlyList<int> ShowNumbers,
    bool OverwriteExisting);

public static class CliHelper
{
    public static bool TryParse(
        string[] args,
        out CliOptions options,
        out string errorMessage,
        out bool showUsage)
    {
        options = new CliOptions(string.Empty, Array.Empty<int>(), false);
        errorMessage = string.Empty;
        showUsage = false;

        var parsed = ParseArgs(args);

        if (args.Length == 0 || parsed.ContainsKey("--help") || parsed.ContainsKey("-h"))
        {
            showUsage = true;
            return false;
        }

        var allowedOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "--output",
            "--shows",
            "--overwrite",
            "--skip-existing",
            "--help",
            "-h"
        };

        var unknownOptions = parsed.Keys.Where(k => !allowedOptions.Contains(k)).ToList();
        if (unknownOptions.Count > 0)
        {
            errorMessage = $"Unknown option(s): {string.Join(", ", unknownOptions)}";
            showUsage = true;
            return false;
        }

        if (!parsed.TryGetValue("--output", out var outputDirectory) || string.IsNullOrWhiteSpace(outputDirectory))
        {
            errorMessage = "Missing required option: --output";
            showUsage = true;
            return false;
        }

        try
        {
            outputDirectory = NormalizeAndEnsureDirectory(outputDirectory);
        }
        catch (Exception ex)
        {
            errorMessage = $"Invalid --output directory '{outputDirectory}': {ex.Message}";
            return false;
        }

        if (!parsed.TryGetValue("--shows", out var showsArg) || string.IsNullOrWhiteSpace(showsArg))
        {
            errorMessage = "Missing required option: --shows";
            showUsage = true;
            return false;
        }

        if (!ConsoleService.TryParseRange(showsArg, out var showNumbers) || showNumbers.Count == 0)
        {
            errorMessage = "No valid show numbers provided in --shows.";
            return false;
        }

        var orderedShowNumbers = showNumbers.Distinct().OrderBy(n => n).ToList();

        var overwrite = parsed.ContainsKey("--overwrite");
        var skipExisting = parsed.ContainsKey("--skip-existing");
        if (overwrite && skipExisting)
        {
            errorMessage = "Options --overwrite and --skip-existing are mutually exclusive.";
            showUsage = true;
            return false;
        }

        options = new CliOptions(outputDirectory, orderedShowNumbers, overwrite);
        return true;
    }

    public static Dictionary<string, string> ParseArgs(string[] args)
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

    public static string ExpandTildePath(string path)
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

    public static string NormalizeAndEnsureDirectory(string path)
    {
        var expanded = ExpandTildePath(path);
        var fullPath = Path.GetFullPath(expanded);
        Directory.CreateDirectory(fullPath);
        return fullPath;
    }
}
