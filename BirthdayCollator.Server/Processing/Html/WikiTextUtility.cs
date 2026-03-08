using BirthdayCollator.Helpers;
using HtmlAgilityPack;
using System.Globalization;
using System.Text;

namespace BirthdayCollator.Server.Processing.Html;

public static class WikiTextUtility
{
    /// <summary>
    /// Cleans Wikipedia's inner text into a standardized format.
    /// </summary>
    public static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        string cleaned = HtmlEntity.DeEntitize(input).Trim();
        cleaned = cleaned.Replace('—', '–').Replace('-', '–');
        cleaned = RegexPatterns.WhitespaceCollapseRegex().Replace(cleaned, " ");
        cleaned = RegexPatterns.OrdinalSuffixRegex().Replace(cleaned, "$1");

        return cleaned;
    }

    public static bool TryExtractBirthYear(string normalizedText, out int birthYear)
    {
        birthYear = 0;
        string[] parts = normalizedText.Split('–', 2);

        if (parts.Length < 2) return false;

        string leftSide = parts[0].Trim();
        string[] tokens = leftSide.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return tokens.Length > 0 && int.TryParse(tokens[^1], out birthYear);
    }

    public static string ExtractPersonName(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        int idx = text.IndexOf('(');
        if (idx > 0)
            text = text[..idx];

        return text.Trim().TrimEnd(',');
    }

    public static string ToComparableSlug(string s)
    {
        s = s.ToLowerInvariant().Replace("_", " ");
        s = s.Normalize(NormalizationForm.FormD);
        IEnumerable<char> chars = s.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
        return new string([.. chars]);
    }

    public static List<string> Tokenize(string s)
    {
        return [.. s
            .Split([' ', ',', '.', '/', '-', '(', ')', '\''], StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .Where(t => t.Length > 0)];
    }

    public static string ExtractDescription(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return string.Empty;

        int dash = rawText.IndexOf('–');
        if (dash >= 0)
            return rawText[(dash + 1)..].Trim();

        int comma = rawText.IndexOf(',');
        if (comma >= 0)
            return rawText[(comma + 1)..].Trim();

        return rawText.Trim();
    }


}
