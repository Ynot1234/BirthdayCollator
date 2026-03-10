using System.Text.RegularExpressions;

namespace BirthdayCollator.Helpers;

public partial class RegexPatterns
{
    // --- 1. Wikipedia & HTML Structure ---
    // Grabs the specific block between Births and Deaths headers
    [GeneratedRegex(@"(?s)Births\s*(.*?)\s*Deaths")]
    public static partial Regex BirthsSectionRegex();

    [GeneratedRegex("<li>(.*?)</li>", RegexOptions.Singleline)]
    public static partial Regex ItemsRegex();

    [GeneratedRegex("<.*?>")]
    public static partial Regex HtmlTagsRegex();

    [GeneratedRegex(@"\{.*?\}", RegexOptions.Singleline)]
    public static partial Regex CurlyBlockRegex();

    // --- 2. Identity & Link Extraction ---
    // Specifically finds the slug and the display name in a Wiki <a> tag
    [GeneratedRegex(@"href=""/wiki/([^""]+)""[^>]*>([^<]+)</a>")]
    public static partial Regex WikiLinkRegex();

    [GeneratedRegex(@"wiki/[^\s\""]+")]
    public static partial Regex WikiLinkFragmentRegex();

    // --- 3. Date & Year Logic ---
    [GeneratedRegex(@"(\d{4})")]
    public static partial Regex YearMatchRegex();

    [GeneratedRegex(@"^[12]\d{3}\b")]
    public static partial Regex StartswithYear();

    [GeneratedRegex(@"\b(\d+)(st|nd|rd|th)\b")]
    public static partial Regex OrdinalSuffixRegex();

    [GeneratedRegex(@"\b(\d{1,2}\s+\w+\s+\d{4}|\w+\s+\d{1,2},\s*\d{4})\b", RegexOptions.IgnoreCase)]
    public static partial Regex LongFormDateRegex();

    [GeneratedRegex(@"([A-Za-z]+)\s+(\d{1,2})")]
    public static partial Regex MonthDayLooseRegex();

    // --- 4. Filtering & Exclusion ---
    // Crucial for your "Living People" logic - finds (died 1990) or (d. 2005)
    [GeneratedRegex(@"\(\s*(died|d\.|†)\s+\d{4}\s*\)", RegexOptions.IgnoreCase)]
    public static partial Regex ExcludeDiedRegex();

    [GeneratedRegex(@"\[\d+\]|\[\s*[^]]+\]")]
    public static partial Regex Citation();

    [GeneratedRegex(@"\([^)]*\)")]
    public static partial Regex Parentheses();

    // --- 5. Text Cleanup ---
    [GeneratedRegex(@"\s+")]
    public static partial Regex WhitespaceCollapseRegex();

    // Extracts the actual content from a JSON response like {"text": {"*": "content"}}
    [GeneratedRegex(@"""text"":\s*\{\s*""\*"":\s*""(.*?)""\s*\}", RegexOptions.Singleline)]
    public static partial Regex ExtractTextStarRegex();
}