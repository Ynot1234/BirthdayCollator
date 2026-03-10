using BirthdayCollator.Server.Constants;
using BirthdayCollator.Helpers; 

namespace BirthdayCollator.Server.Processing.Html;

public static partial class WikiTextUtility
{
    public static bool TryExtractBirthYear(string normalizedText, out int birthYear)
    {
        birthYear = 0;
        string[] parts = normalizedText.Split('–', 2);
        if (parts.Length < 2) return false;
        var tokens = parts[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return tokens.Length > 0 && int.TryParse(tokens[^1], out birthYear);
    }

    public static string? ExtractBioParenthetical(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        int start = 0;
        while ((start = text.IndexOf('(', start)) >= 0)
        {
            int end = text.IndexOf(')', start + 1);
            if (end < 0) break;
            string content = text[start..(end + 1)];
            if (MonthNames.All.Any(m => content.Contains(m, StringComparison.OrdinalIgnoreCase)) || content.Contains('–'))
                return content;
            start = end + 1;
        }
        return null;
    }

    public static bool IndicatesDeath(string? paren, DateTime birthDate)
    {
        if (string.IsNullOrEmpty(paren)) return false;

        if (paren.Contains('–') && !paren.Contains("present", StringComparison.OrdinalIgnoreCase))
            return true;

        return IsDateMismatch(paren, birthDate);
    }

    public static bool IsDateMismatch(string text, DateTime birthDate)
    {
        var match = RegexPatterns.LongFormDate().Match(text);
        return match.Success && DateTime.TryParse(match.Value, out var parsed)
               && (parsed.Month != birthDate.Month || parsed.Day != birthDate.Day);
    }
}