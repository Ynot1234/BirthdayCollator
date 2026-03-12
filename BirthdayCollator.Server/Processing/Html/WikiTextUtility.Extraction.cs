using BirthdayCollator.Helpers;
using HtmlAgilityPack;

namespace BirthdayCollator.Server.Processing.Html;

public static partial class WikiTextUtility
{
    public static string ExtractDescription(string rawText, string? personName = null)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return string.Empty;

        var lines = SplitAndCleanLines(rawText);
        string targetLine = SelectTargetLine(lines, rawText);

        string description = ExtractCoreDescription(targetLine);
        description = RemovePersonName(description, personName);
        description = RemoveTitles(description);
        description = RemovePrefixes(description);
        description = StripLeadingVerbs(description);

        string final = ExtractFirstSentence(description);

        return string.IsNullOrWhiteSpace(final) || final.Length < 3
            ? targetLine.Trim()
            : final;
    }

    public static string? GetFirstBioParagraph(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        HtmlDocument doc = new();
        doc.LoadHtml(html);

        var shortDescNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'shortdescription')]");
        if (shortDescNode != null)
        {
            string text = HtmlEntity.DeEntitize(shortDescNode.InnerText).Trim();
            return RegexPatterns.DisplayCleaner().Replace(text, "").Trim();
        }

        var pNode = doc.DocumentNode.SelectSingleNode("//p[b]");
        if (pNode != null)
        {
            string text = HtmlEntity.DeEntitize(pNode.InnerText).Trim();
            return RegexPatterns.DisplayCleaner().Replace(text, "").Trim();
        }

        return null;
    }

    public static string? GetRawFirstParagraph(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        HtmlDocument doc = new();
        doc.LoadHtml(html);

        var pNode = doc.DocumentNode.SelectSingleNode("//p[b]");
        pNode ??= doc.DocumentNode.SelectSingleNode(
            "//p[not(contains(@class,'mw-empty-elt')) and string-length(normalize-space()) > 20]"
        );

        if (pNode == null)
            return null;

        return HtmlEntity.DeEntitize(pNode.InnerText).Trim();
    }

    
}