using System.Text;
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
    /// Prompts the user to enter show numbers or ranges, parses the input, and returns a list of integers.
    /// Returns an empty list if the input is invalid.
    /// </summary>
    public static List<int> PromptForShowNumbers()
    {
        var input = GetInputWithDefault("Enter show numbers or ranges (e.g., 1,3-5): ", "");

        if (TryParseRange(input, out var numbers))
            return numbers;

        AnsiConsole.MarkupLine("[yellow]Invalid input. No shows selected.[/]");
        return [];
    }

    /// <summary>
    /// Prompts the user to confirm downloading the given shows.
    /// Returns true if the user confirms with 'y', false otherwise.
    /// </summary>
    public static bool ConfirmDownload(List<int> showNumbers)
    {
        var orderedShows = showNumbers.OrderBy(n => n).ToList();
        string showsDisplay;

        if (orderedShows.Count > 10)
        {
            var firstThree = string.Join(", ", orderedShows.Take(3));
            var lastThree = string.Join(", ", orderedShows.TakeLast(3));
            showsDisplay = $"{firstThree}, ..., {lastThree}";
        }
        else
        {
            showsDisplay = string.Join(", ", orderedShows);
        }

        AnsiConsole.MarkupLine($"[blue]Proceed to download {orderedShows.Count} shows: {showsDisplay}? (y/n):[/]");

        var response = AnsiConsole.Prompt(
            new TextPrompt<string>("[grey](y/n)[/]")
                .AllowEmpty()
                .DefaultValue("n")
        );

        return response.Trim().Equals("y", StringComparison.CurrentCultureIgnoreCase);
    }

    /// <summary>
    /// Prompts the user for a download directory, returning the default if input is blank.
    /// </summary>
    public static string PromptForDownloadDirectory(string defaultDirectory) =>
        GetInputWithDefault("Enter download directory: ", defaultDirectory);

    /// <summary>
    /// Prompts the user for the maximum number of parallel downloads, with validation.
    /// </summary>
    public static int PromptForMaxParallelDownloads(int defaultValue)
    {
        var result = GetInputWithDefault("Enter max parallel downloads:", defaultValue.ToString());

        if (int.TryParse(result, out var n) && n > 0)
            return n;

        return defaultValue;
    }

    /// <summary>
    /// Logs an info message in blue.
    /// </summary>
    public static void LogInfo(string message) =>
        AnsiConsole.MarkupLine($"[blue]{message}[/]");

    /// <summary>
    /// Logs a warning message in yellow.
    /// </summary>
    public static void LogWarning(string message) =>
        AnsiConsole.MarkupLine($"[yellow]{message}[/]");

    /// <summary>
    /// Logs an error message in red.
    /// </summary>
    public static void LogError(string message) =>
        AnsiConsole.MarkupLine($"[red]{message}[/]");

    /// <summary>
    /// Logs a message indicating the SoundCloud client has been initialized.
    /// </summary>
    public static void LogClientInitialized(string clientId) =>
        AnsiConsole.MarkupLine($"[green]SoundCloud client initialized with client ID: {clientId}[/]");

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

    private static string GetInputWithDefault(string prompt, string defaultValue)
    {
        Console.Write(prompt.EndsWith(' ') ? prompt : prompt + " ");
        
        var input = new StringBuilder(defaultValue);
        // Use correct ANSI escape codes for green text
        Console.Write("\u001b[32m" + defaultValue + "\u001b[0m");
        var cursorPos = defaultValue.Length;
        ConsoleKeyInfo key;
        do
        {
            key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Backspace && cursorPos > 0)
            {
                input.Remove(cursorPos - 1, 1);
                cursorPos--;
                Console.CursorLeft = prompt.Length;
                // Clear line
                Console.Write(new string(' ', input.Length + 1));
                Console.CursorLeft = prompt.Length;
                Console.Write("\u001b[32m" + input + "\u001b[0m");
                Console.CursorLeft = prompt.Length + cursorPos;
            }
            else if (!char.IsControl(key.KeyChar))
            {
                input.Insert(cursorPos, key.KeyChar);
                cursorPos++;
                Console.Write("\u001b[32m" + input.ToString(cursorPos - 1, input.Length - (cursorPos - 1)) + "\u001b[0m");
                Console.CursorLeft = prompt.Length + cursorPos;
            }
            else if (key.Key == ConsoleKey.LeftArrow && cursorPos > 0)
            {
                cursorPos--;
                Console.CursorLeft = prompt.Length + cursorPos;
            }
            else if (key.Key == ConsoleKey.RightArrow && cursorPos < input.Length)
            {
                cursorPos++;
                Console.CursorLeft = prompt.Length + cursorPos;
            }
        } while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return input.ToString();
    }
}