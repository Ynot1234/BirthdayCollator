using BirthdayCollator.Helpers;
using System.Globalization;
using System.Text;

namespace BirthdayCollator.Server.Helpers;

public static class StringNormalization
{
    public static string CleanName(string name)
    {
        return name.Trim().TrimEnd(',');
    }

    public static string RemoveParenthetical(string text)
    {
        int idx = text.IndexOf('(');
        return idx > 0 ? text[..idx].Trim() : text.Trim();
    }

    public static (string name, string? description) SplitNameAndDescription(string text)
    {
        int commaIndex = text.IndexOf(',');
        if (commaIndex < 0) return (text.Trim(), null);
        return ( text[..commaIndex].Trim(), text[(commaIndex + 1)..].Trim());
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

        string result = new([.. chars]);
        return RegexPatterns.WhitespaceCollapse().Replace(result, " ").Trim();
    }

    public static string NormalizeWikiUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return string.Empty;

        return url.Trim().ToLowerInvariant()
            .Replace("_,jr.", "_jr.")
            .Replace("jr.", "jr")
            .Replace(",", "");
    }




    private static readonly char[] _debris = [' ', ',', '.', ';', ':', '-', '–'];

    public static string TrimDebris(this string? text)
    {
        return text?.TrimStart(_debris).Trim() ?? string.Empty;
    }

}