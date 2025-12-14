using Spectre.Console;

namespace DsohScraper.Services;

public static class ConsoleService
{
    /// <summary>
    /// Parses a string containing numbers and ranges (e.g., "1,3-5") into a list of integers.
    /// Returns true if parsing is successful, false otherwise.
    /// All numbers must be greater than 100 and less than 2000.
    /// </summary>
    public static bool TryParseRange(string input, out List<int> numbers)
    {
        numbers = [];
        
        if (string.IsNullOrWhiteSpace(input))
            return false;
        
        try
        {
            foreach (var part in input.Split(','))
            {
                if (part.Contains('-'))
                {
                    var range = part.Split('-');
                    if (range.Length == 2 && int.TryParse(range[0], out var start) && int.TryParse(range[1], out var end))
                    {
                        for (var i = start; i <= end; i++)
                        {
                            if (i is > 100 and < 2000)
                                numbers.Add(i);
                            else
                                return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (int.TryParse(part, out var num))
                {
                    if (num is > 100 and < 2000)
                        numbers.Add(num);
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }
            return numbers.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Logs an info message in blue.
    /// </summary>
    public static void LogInfo(string message) =>
        AnsiConsole.MarkupLine($"[blue]{Markup.Escape(message)}[/]");

    /// <summary>
    /// Logs a warning message in yellow.
    /// </summary>
    public static void LogWarning(string message) =>
        AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(message)}[/]");

    /// <summary>
    /// Logs an error message in red.
    /// </summary>
    public static void LogError(string message) =>
        AnsiConsole.MarkupLine($"[red]{Markup.Escape(message)}[/]");

    /// <summary>
    /// Logs a message indicating the SoundCloud client has been initialized.
    /// </summary>
    public static void LogClientInitialized(string clientId) =>
        AnsiConsole.MarkupLine($"[green]{Markup.Escape($"SoundCloud client initialized with client ID: {clientId}")}[/]");

    /// <summary>
    /// Logs a warning if the SoundCloud client initialization fails.
    /// </summary>
    public static void LogClientInitializationError(string errorMessage) =>
        LogWarning($"Failed to initialize SoundCloud client: {errorMessage}. Using default client ID.");

    /// <summary>
    /// Logs an error if track URL extraction fails for a show.
    /// </summary>
    public static void LogTrackUrlExtractionFailed(int showNumber) =>
        LogError($"Failed to extract track URL for show #{showNumber}.");
}