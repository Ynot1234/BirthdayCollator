using BirthdayCollator.Server.Constants;
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
}