using BirthdayCollator.Helpers;
using BirthdayCollator.Server.Constants;
using HtmlAgilityPack;

namespace BirthdayCollator.Server.Processing.Html;

public static partial class WikiTextUtility
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
   
   public static string SanitizeWikiText(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        string text = RegexPatterns.Citation().Replace(input, "");
        text = RegexPatterns.Parentheses().Replace(text, "");
        return ExtractDescription(text).Replace("  ", " ");
    }
}
