using BirthdayCollator.Helpers;
using BirthdayCollator.Server.Constants;
using HtmlAgilityPack;
using System.Globalization;
using System.Text;

namespace BirthdayCollator.Server.Processing.Html;

public static class WikiTextUtility
{
    public static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        string cleaned = HtmlEntity.DeEntitize(input).Trim();
        cleaned = cleaned.Replace('—', '–').Replace('-', '–');
        cleaned = RegexPatterns.WhitespaceCollapseRegex().Replace(cleaned, " ");
        cleaned = RegexPatterns.OrdinalSuffixRegex().Replace(cleaned, "$1");

        return cleaned;
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

            if (ContainsMonth(content) || content.Contains('–'))
                return content;

            start = end + 1;
        }
        return null;
    }

    private static bool ContainsMonth(string s) =>
        MonthNames.All.Any(m => s.Contains(m, StringComparison.OrdinalIgnoreCase));


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
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
       
        s = s.ToLowerInvariant().Replace("_", " ");
        s = s.Normalize(NormalizationForm.FormD);
        var chars = s.Where(c =>
            CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark &&
            !char.IsPunctuation(c) &&
            !char.IsSymbol(c)
        );

        string result = new string([.. chars]);
        return RegexPatterns.WhitespaceCollapseRegex().Replace(result, " ").Trim();
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

    public static string SanitizeWikiText(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) 
            return string.Empty;
 
        string text = RegexPatterns.Citation().Replace(input, "");
        text = RegexPatterns.Parentheses().Replace(text, "");
        return ExtractDescription(text).Replace("  ", " ");
    }
    public static string GetFirstTwoWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (words.Length >= 2)
            return $"{words[0]} {words[1]}";

        return words.Length > 0 ? words[0] : string.Empty;
    }

    public static string GetFirstName(string name) =>
    string.IsNullOrWhiteSpace(name) ? "" : name.Split(' ')[0].ToLowerInvariant();

    public static bool HasKeywordOverlap(string? a, string? b)
    {
        var wa = ExtractKeywords(a);
        var wb = ExtractKeywords(b);
        return wa.Intersect(wb).Count() >= 2;
    }

    private static HashSet<string> ExtractKeywords(string? text) =>
        string.IsNullOrWhiteSpace(text) ? [] :
        [.. text.ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(w => w.Trim(',', '.', ';', ':', '!', '?', '(', ')', '[', ']'))
            .Where(w => w.Length >= 3 && !NameParsing.Stopwords.Contains(w))];


    public static string NormalizeDescription(string text)
    {
        // 1. General cleanup (Dashes, Entities, Whitespace)
        text = Normalize(text);
        if (string.IsNullOrWhiteSpace(text)) return text;

        // 2. Strip prefixes (a, an, the)
        foreach (var prefix in NameParsing.Prefixes)
        {
            if (text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                text = text[prefix.Length..].TrimStart();
                break;
            }
        }

        // 3. Sentence case (Capitalize first letter)
        return text.Length > 0
            ? char.ToUpper(text[0]) + text[1..]
            : text;
    }

}