using System.Text.RegularExpressions;

namespace BirthdayCollator.Server.Helpers;

public partial class RegexPatterns
{
    [GeneratedRegex(@"\b(\d+)(st|nd|rd|th)\b")]
    public static partial Regex OrdinalSuffix();

    [GeneratedRegex(@"\b(\d{1,2}\s+\w+\s+\d{4}|\w+\s+\d{1,2},\s*\d{4})\b", RegexOptions.IgnoreCase)]
    public static partial Regex LongFormDate();

    [GeneratedRegex(@"([A-Za-z]+)\s+(\d{1,2})")]
    public static partial Regex MonthDayLoose();

    [GeneratedRegex(@"\(\s*.*?\d{4}\s*\)", RegexOptions.IgnoreCase)]
    public static partial Regex ExcludeDied();

    [GeneratedRegex(@"\[\d+\]|\[\s*[^]]+\]")]
    public static partial Regex Citation();

    [GeneratedRegex(@"\([^)]*\)")]
    public static partial Regex Parentheses();

    [GeneratedRegex(@"\s+")]
    public static partial Regex WhitespaceCollapse();

    [GeneratedRegex(@"(?<=\d{4}|\s)[–-]\s*.*?(?<deathYear>\d{4})|died\s+(?<diedYear>\d{4})", RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex DeathYearMarker();

    [GeneratedRegex(@"\s*\(.*?\)")]
    public static partial Regex DisplayCleaner();

    [GeneratedRegex(@"(?<!\b[A-Z]|Mr|St|Dr)[.;]")]
    public static partial Regex SentenceBoundary();

    [GeneratedRegex(@"\(([^)]*?(\d{4}|born|died)[^)]*?)\)", RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex BioParenthetical();

    [GeneratedRegex(@"\b(January|February|March|April|May|June|July|August|September|October|November|December)\b", RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex MonthName();

    [GeneratedRegex(@"\b\d{4}\b")]
    public static partial Regex YearIndicator();

    [GeneratedRegex(@"\b(?:was\s+)?born\s+on\s+.*?\.", RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex ExcludeBirthStatement();
    [GeneratedRegex(@"\s*See full bio.*$", RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex BioLinkSuffix();
    [GeneratedRegex(@"^\d+\.\s+")]
    public static partial Regex BioLinkTail();
    [GeneratedRegex(@"\s*See full bio.*$", RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex ExcludeImdbFooter();

    [GeneratedRegex(@"^\d+\.\s+")]
    public static partial Regex LeadingRank();

    [GeneratedRegex(@"^was born on.*?\d{4}.*?\.\s*", RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex LeadingBirthStatement();

    [GeneratedRegex(@"^(He|She|Who|\w+)\s+known for\s*", RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex KnownForPrefix();

    [GeneratedRegex(@"^(\s*,\s*|\s*and\s+)", RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex LeadingConnectors();

    [GeneratedRegex(@"\s{2,}")]
    public static partial Regex CollapseWhitespace();

    [GeneratedRegex(@"\bis\s+|\bwas\s+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    public static partial Regex VerbPrefixRegex();
}