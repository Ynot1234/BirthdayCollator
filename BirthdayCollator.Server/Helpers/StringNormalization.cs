using System.Globalization;
using System.Text;

namespace BirthdayCollator.Server.Helpers;
public static class StringNormalization
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
        return RegexPatterns.WhitespaceCollapse().Replace(result, " ").Trim();
    }

    private static readonly char[] _debris = [' ', ',', '.', ';', ':', '-', '–'];

    public static string TrimDebris(this string? text)
    {
        return text?.TrimStart(_debris).Trim() ?? string.Empty;
    }
}