using System.Globalization;
using System.Text;

namespace DsohScraper.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Converts a string to kebab-case, removes non-ASCII characters, and replaces spaces/special chars with '-'.
    /// </summary>
    public static string ToAsciiKebabCase(this string input)
    {
        // Normalize and remove diacritics
        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        var prevChar = '\0';
        
        foreach (var c in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory == UnicodeCategory.NonSpacingMark)
                continue;

            // ASCII only
            if (c > 127) 
                continue;
            
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(char.ToLowerInvariant(c));
                prevChar = c;
            }
            else if (c is ' ' or '_' or '-')
            {
                if (sb.Length > 0 && prevChar != '-')
                {
                    sb.Append('-');
                    prevChar = '-';
                }
            }
        }
        
        // Remove trailing dash if present
        return sb.ToString().Trim('-');
    }
}
