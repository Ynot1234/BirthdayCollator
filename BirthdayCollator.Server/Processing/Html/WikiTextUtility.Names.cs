using BirthdayCollator.Helpers;
using BirthdayCollator.Server.Constants;
using System.Globalization;
using System.Text;

namespace BirthdayCollator.Server.Processing.Html;

public static partial class WikiTextUtility
{
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

        string result = new([.. chars]);
        return RegexPatterns.WhitespaceCollapseRegex().Replace(result, " ").Trim();
    }

    public static string ExtractPersonName(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        int idx = text.IndexOf('(');
        if (idx > 0) text = text[..idx];

        return text.Trim().TrimEnd(',');
    }

    public static string GetFirstName(string name) =>
        string.IsNullOrWhiteSpace(name) ? "" : name.Split(' ')[0].ToLowerInvariant();

    public static string GetFirstTwoWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length >= 2 ? $"{words[0]} {words[1]}" : (words.Length > 0 ? words[0] : string.Empty);
    }

    public static List<string> Tokenize(string s)
    {
        return [.. s
            .Split([' ', ',', '.', '/', '-', '(', ')', '\''], StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .Where(t => t.Length > 0)];
    }

    public static bool FuzzyNameMatch(string a, string b)
    {
        List<string> aTokens =
            [.. Tokenize(ToComparableSlug(a))
            .Except(NameParsing.Stopwords)
            .Except(NameParsing.Titles)];

        List<string> bTokens =
            [.. Tokenize(ToComparableSlug(b))
            .Except(NameParsing.Stopwords)
            .Except(NameParsing.Titles)];

        return aTokens.Count > 0 &&
               bTokens.Count > 0 &&
               aTokens.Any(at =>
                   bTokens.Any(bt =>
                       bt.Contains(at, StringComparison.OrdinalIgnoreCase) ||
                       at.Contains(bt, StringComparison.OrdinalIgnoreCase)));
    }


}