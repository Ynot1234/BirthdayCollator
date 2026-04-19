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

    [GeneratedRegex(@"\([^)]*\)")]
    public static partial Regex Parentheses();

    [GeneratedRegex(@"\s+")]
    public static partial Regex WhitespaceCollapse();

    [GeneratedRegex(@"\s{2,}")]
    public static partial Regex CollapseWhitespace();

    [GeneratedRegex(
     @"(?:[-\p{Pd}]\s*.*?(?<deathYear>1[0-9]{3}|20[0-9]{2}))|died\s+(?<diedYear>1[0-9]{3}|20[0-9]{2})",
     RegexOptions.IgnoreCase | RegexOptions.Singleline)]
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

    [GeneratedRegex(@"\b(19|20)\d{2}\b")]
    public static partial Regex YearCandidate();

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

   

    [GeneratedRegex(@"\bis\s+|\bwas\s+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    public static partial Regex VerbPrefixRegex();

    [GeneratedRegex(@"nm\d+")]
    public static partial Regex PersonId();

    [GeneratedRegex(@"(?<=\b\p{L})\.(?=\p{L}\b)")]
    public static partial Regex SplitInitials();

    [GeneratedRegex("[\"'()]")]
    public static partial Regex RemoveQuotesAndParens();
    [GeneratedRegex(@"[^\p{L}\p{N}\s]")]
    public static partial Regex RemoveNonAlphanumeric();

    [GeneratedRegex(@"\s+")]
    public static partial Regex TokenizeCanonical();

    [GeneratedRegex(@"[\[\]\(\),]")]
    public static partial Regex BoundaryMarkers();

    [GeneratedRegex(@"\s*[\(\[\{].*?[\)\]\}]\s*")]
    public static partial Regex ParentheticalRemover();

    [GeneratedRegex(@"\s+")]
    public static partial Regex CondensedSpace();

    [GeneratedRegex(@"<script[^>]*id=""__NEXT_DATA__""[^>]*>(.*?)</script>", RegexOptions.Singleline)]
    public static partial Regex ExtractNextDataJson();

}