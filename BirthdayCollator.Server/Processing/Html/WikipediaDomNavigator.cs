namespace BirthdayCollator.Server.Processing.Html;

using BirthdayCollator.Server.Constants;
using HtmlAgilityPack;
//using global::BirthdayCollator.Server.Constants;


public static class WikipediaDomNavigator
{
    /// <summary>
    /// Finds the "Births" section and extracts all relevant list items (li nodes).
    /// </summary>
    public static List<HtmlNode> ExtractBirthLiNodes(HtmlDocument htmlDoc)
    {
        HtmlNode birthsHeader = htmlDoc.DocumentNode.SelectSingleNode(XPathSelectors.YearBirthsHeader);
        if (birthsHeader == null) return [];

        List<HtmlNode> sections = [];
        // Walk siblings until the next major header (h2)
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

        return doc.DocumentNode
            .SelectSingleNode("//div[@class='mw-parser-output']/p[not(@class='mw-empty-elt')]")
            ?.InnerText;
    }


    /// <summary>
    /// Filters through links in a list item to find the specific person's wiki link.
    /// </summary>
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
}

