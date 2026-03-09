using HtmlAgilityPack;


namespace BirthdayCollator.Server.Processing.Html;

public static partial class WikiTextUtility
{
    public static string ExtractDescription(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return string.Empty;

        int dash = rawText.IndexOf('–');
        if (dash >= 0) return rawText[(dash + 1)..].Trim();

        int comma = rawText.IndexOf(',');
        if (comma >= 0) return rawText[(comma + 1)..].Trim();

        return rawText.Trim();
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
}