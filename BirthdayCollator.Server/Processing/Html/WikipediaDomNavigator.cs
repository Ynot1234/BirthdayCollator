using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Processing.Builders;
using HtmlAgilityPack;

namespace BirthdayCollator.Server.Processing.Html;


public static class WikipediaDomNavigator
{
    public static List<HtmlNode> ExtractBirthLiNodes(HtmlDocument htmlDoc)
    {
        HtmlNode birthsHeader = htmlDoc.DocumentNode.SelectSingleNode(XPathSelectors.YearBirthsHeader);
        if (birthsHeader == null) return [];

        List<HtmlNode> sections = [];
        
        for (HtmlNode node = birthsHeader.NextSibling; node != null; node = node.NextSibling)
        {
            if (node.Name == "h2") break;
            if (node.Name is "section" or "div" or "ul") sections.Add(node);
        }

        return [.. sections.SelectMany(s => s.SelectNodes(".//li") ?? Enumerable.Empty<HtmlNode>())];
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

  public static HtmlNode? TryFindPersonLink(HtmlNode liNode)
    {
        var links = liNode.SelectNodes(XPathSelectors.DescendantAnchorHref);
        if (links == null) return null;

        foreach (var link in links)
        {
            string href = link.GetAttributeValue("href", string.Empty);
            if (IsYearLink(href) || IsMonthDayLink(href)) continue;
            return link;
        }

        return null;
    }

    private static bool IsYearLink(string href)
    {
        string trimmed = href.TrimStart('.', '/');
        return trimmed.Length == 4 && int.TryParse(trimmed, out _);
    }

    private static bool IsMonthDayLink(string href)
    {
        string[] parts = href.Split('_');
        return parts.Length == 2 &&
               DateTime.TryParse($"{parts[0]} 1", out _) &&
               int.TryParse(parts[1], out _);
    }

    public static string? ExtractWikipediaHref(HtmlNode node)
    {
        var url = node.SelectSingleNode(".//a[contains(@href,'wikipedia.org')]")
                      ?.GetAttributeValue("href", "");

        return string.IsNullOrWhiteSpace(url) ? null : WikiUrlBuilder.NormalizeWikiHref(url);
    }

}