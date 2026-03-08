using BirthdayCollator.Helpers;
using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Processing.Pipelines;
using HtmlAgilityPack;
using Microsoft.VisualBasic;
using System;
using System.Globalization;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

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