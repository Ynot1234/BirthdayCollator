using System.Text.RegularExpressions;

namespace BirthdayCollator.Helpers
{
    public partial class RegexPatterns
    {
        [GeneratedRegex("\"\\*\":\"(.*?)\"\\}\\}\\}$", RegexOptions.Singleline)]
        public static partial Regex StarFieldCaptureRegex();

        [GeneratedRegex("<li>(.*?)</li>", RegexOptions.Singleline)]
        public static partial Regex ItemsRegex();

        [GeneratedRegex("<a href=\"(/wiki/[^\"]+)\"[^>]*>(.*?)</a>")]
        public static partial Regex LinkMatchRegex();

        [GeneratedRegex(@"\s+")]
        public static partial Regex WhitespaceCollapseRegex();

        [GeneratedRegex(@"\b(\d+)(st|nd|rd|th)\b")]
        public static partial Regex OrdinalSuffixRegex();

        [GeneratedRegex(@"(\d{4})")]
        public static partial Regex YearMatchRegex();

        [GeneratedRegex("<.*?>")]
        public static partial Regex HtmlTagsRegex();

        [GeneratedRegex(@"([A-Za-z]+)\s+(\d{1,2})")]
        public static partial Regex MonthDayLooseRegex();

        [GeneratedRegex(@"[\(\)\[\]]")]
        public static partial Regex RemoveParanthsesBracketsRegex();

        [GeneratedRegex(@"\d")]
        public static partial Regex RemoveDigitsRegex();

        [GeneratedRegex(@"\s{2,}")]
        public static partial Regex NormalizeSpacesRegex();

        [GeneratedRegex(@"https?:\/\/\S+")]
        public static partial Regex RemoveTrailingURL();  

        [GeneratedRegex(@"^\\u([0-9A-Fa-f]{4})?")]
        public static partial Regex RemoveLeadingU();

        [GeneratedRegex(@"href=""/wiki/([^""]+)""[^>]*>([^<]+)</a>", RegexOptions.Compiled)]
        public static partial Regex WikiLinkRegex();


        [GeneratedRegex(@"^" + Months + @"\s+\d{1,2}\b", RegexOptions.Compiled)]
        public static partial Regex StartsWithFullMonthDayRegex();

        [GeneratedRegex(@"\b" + Months + @"\s+\d{1,2}\b", RegexOptions.IgnoreCase, "en-US")]
        public static partial Regex MonthsRegex();

        [GeneratedRegex(@"^\s*" + Months + @"\s+\d{1,2}\s*$")]
        public static partial Regex IsDateRegex();

        [GeneratedRegex(@"""text"":\s*\{\s*""\*"":\s*""(.*?)""\s*\}", RegexOptions.Singleline)]
        public static partial Regex ExtractTextStarRegex();

        [GeneratedRegex(@"\(\s*(died|d\.|†)\s+\d{4}\s*\)", RegexOptions.IgnoreCase, "en-US")]
        public static partial Regex ExcludeDiedRegex();

        [GeneratedRegex(@"\[[^\]]*\]")]
        public static partial Regex RemoveBracketedContent();

        [GeneratedRegex(@"^[12]\d{3}\b")]
        public static partial Regex StartswithYear();

        // Removes {...} blocks (templates, metadata, etc.)
        [GeneratedRegex(@"\{.*?\}", RegexOptions.Singleline)]
        public static partial Regex CurlyBlockRegex();

        // Captures the Births → Deaths section
        [GeneratedRegex(@"(?s)Births\s*(.*?)\s*Deaths")]
        public static partial Regex BirthsSectionRegex();

        // Removes [123] style reference markers
        [GeneratedRegex(@"\[\d+\]")]
        public static partial Regex ReferenceBracketRegex();

        // Extracts wiki/Some_Page fragments
        [GeneratedRegex(@"wiki/[^\s\""]+")]
        public static partial Regex WikiLinkFragmentRegex();

        [GeneratedRegex(@"\[\s*[^]]+\]")]
        public static partial Regex Citation();

        [GeneratedRegex(@"\b(\d{1,2}\s+\w+\s+\d{4}|\w+\s+\d{1,2},\s*\d{4})\b", 
          RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
        public static partial Regex LongFormDateRegex();

        private const string Months = "(January|February|March|April|May|June|July|August|September|October|November|December)";

    }
}