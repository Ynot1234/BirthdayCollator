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

    public static string? GetFirstBioParagraph(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return null;

        HtmlDocument doc = new();
        doc.LoadHtml(html);

        var node = doc.DocumentNode.SelectSingleNode(
            "//body[contains(@class,'mw-parser-output')]//p[not(contains(@class,'shortdescription')) and normalize-space()]"
        );

        return node != null ? HtmlEntity.DeEntitize(node.InnerText).Trim() : null;
    }

    public static string NormalizeDescription(string text)
    {
        text = Normalize(text);
        if (string.IsNullOrWhiteSpace(text)) return text;

        foreach (var prefix in NameParsing.Prefixes)
        {
            if (text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                text = text[prefix.Length..].TrimStart();
                break;
            }
        }

        return text.Length > 0 ? char.ToUpper(text[0]) + text[1..] : text;
    }

    public static string ExtractDescription(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return string.Empty;

        int dash = rawText.IndexOf('–');
        if (dash >= 0) return rawText[(dash + 1)..].Trim();

        int comma = rawText.IndexOf(',');
        if (comma >= 0) return rawText[(comma + 1)..].Trim();

        return rawText.Trim();
    }

    public static string SanitizeWikiText(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        string text = RegexPatterns.Citation().Replace(input, "");
        text = RegexPatterns.Parentheses().Replace(text, "");
        return ExtractDescription(text).Replace("  ", " ");
    }
}
