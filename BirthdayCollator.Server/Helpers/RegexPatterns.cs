using System.Text.RegularExpressions;

namespace BirthdayCollator.Helpers;

public partial class RegexPatterns
{
    private const string Months = "(January|February|March|April|May|June|July|August|September|October|November|December)";

    // --- Section & Structure Extraction ---
    [GeneratedRegex(@"(?s)Births\s*(.*?)\s*Deaths")]
    public static partial Regex BirthsSectionRegex();

    [GeneratedRegex("<li>(.*?)</li>", RegexOptions.Singleline)]
    public static partial Regex ItemsRegex();

    // --- Cleaning & Sanitization ---
    [GeneratedRegex(@"\s+")]
    public static partial Regex WhitespaceCollapseRegex();

    [GeneratedRegex(@"\[\d+\]|\[\s*[^]]+\]")] // Combines Reference and Citation
    public static partial Regex Citation();

    [GeneratedRegex(@"\([^)]*\)")]
    public static partial Regex Parentheses();

    [GeneratedRegex("<.*?>")]
    public static partial Regex HtmlTagsRegex();

    [GeneratedRegex(@"\{.*?\}", RegexOptions.Singleline)]
    public static partial Regex CurlyBlockRegex();

    // --- Identity & Links ---
    [GeneratedRegex(@"href=""/wiki/([^""]+)""[^>]*>([^<]+)</a>")]
    public static partial Regex WikiLinkRegex();

    [GeneratedRegex(@"wiki/[^\s\""]+")]
    public static partial Regex WikiLinkFragmentRegex();

    // --- Date & Year Parsing ---
    [GeneratedRegex(@"(\d{4})")]
    public static partial Regex YearMatchRegex();

    [GeneratedRegex(@"^[12]\d{3}\b")]
    public static partial Regex StartswithYear();

    [GeneratedRegex(@"\b(\d+)(st|nd|rd|th)\b")]
    public static partial Regex OrdinalSuffixRegex();

    [GeneratedRegex(@"\b(\d{1,2}\s+\w+\s+\d{4}|\w+\s+\d{1,2},\s*\d{4})\b", RegexOptions.IgnoreCase)]
    public static partial Regex LongFormDateRegex();

    [GeneratedRegex(@"\(\s*(died|d\.|†)\s+\d{4}\s*\)", RegexOptions.IgnoreCase)]
    public static partial Regex ExcludeDiedRegex();

    // --- API Parsing ---
    [GeneratedRegex(@"""text"":\s*\{\s*""\*"":\s*""(.*?)""\s*\}", RegexOptions.Singleline)]
    public static partial Regex ExtractTextStarRegex();


    [GeneratedRegex(@"([A-Za-z]+)\s+(\d{1,2})")]
    public static partial Regex MonthDayLooseRegex();

}
