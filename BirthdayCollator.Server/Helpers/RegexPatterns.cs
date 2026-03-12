using System.Text.RegularExpressions;

namespace BirthdayCollator.Helpers;

public partial class RegexPatterns
{
    [GeneratedRegex(@"\b(\d+)(st|nd|rd|th)\b")]
    public static partial Regex OrdinalSuffix();

    [GeneratedRegex(@"\b(\d{1,2}\s+\w+\s+\d{4}|\w+\s+\d{1,2},\s*\d{4})\b", RegexOptions.IgnoreCase)]
    public static partial Regex LongFormDate();

    [GeneratedRegex(@"([A-Za-z]+)\s+(\d{1,2})")]
    public static partial Regex MonthDayLoose();

    [GeneratedRegex(@"\(\s*(died|d\.|†)\s+\d{4}\s*\)", RegexOptions.IgnoreCase)]
    public static partial Regex ExcludeDied();

    [GeneratedRegex(@"\[\d+\]|\[\s*[^]]+\]")]
    public static partial Regex Citation();

    [GeneratedRegex(@"\([^)]*\)")]
    public static partial Regex Parentheses();

    [GeneratedRegex(@"\s+")]
    public static partial Regex WhitespaceCollapse();

    [GeneratedRegexAttribute(@"[–-]\s*(?:[A-Za-z]+\s+\d{1,2},\s+)?(\d{4})|died\s+(\d{4})", RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex DeathYearMarker();

    [GeneratedRegex(@"\s*\(.*?\)")]
    public static partial Regex DisplayCleaner();

    [GeneratedRegex(@"(?<!\b[A-Z]|Mr|St|Dr)[.;]")]
    public static partial Regex SentenceBoundary();
}