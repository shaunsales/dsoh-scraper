using DsohScraper.Extensions;

namespace DsohScraper.Helpers;

public static class FileNameHelper
{
    private const int MaxFileNameBaseLength = 120;

    public static string BuildMp3Path(string outputDirectory, int showNumber, string? trackTitle)
    {
        var kebabTitle = trackTitle?.ToAsciiKebabCase() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(kebabTitle))
            kebabTitle = $"dsoh-{showNumber}-deeper-shades-of-house";

        if (kebabTitle.Length > MaxFileNameBaseLength)
            kebabTitle = kebabTitle[..MaxFileNameBaseLength];

        return Path.Combine(outputDirectory, $"{kebabTitle}.mp3");
    }
}
