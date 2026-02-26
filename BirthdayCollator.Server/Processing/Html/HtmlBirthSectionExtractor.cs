using BirthdayCollator.Helpers;
using HtmlAgilityPack;

namespace BirthdayCollator.Server.Processing.Html;

public interface IHtmlBirthSectionExtractor
{
    List<HtmlNode> ExtractLiNodes(string html, string xpath);
    string? ExtractPersonHref(string innerHtml);
    string? ExtractHrefForEntry(HtmlNode li, string personName);
}

public sealed class HtmlBirthSectionExtractor : IHtmlBirthSectionExtractor
{
    public List<HtmlNode> ExtractLiNodes(string html, string xpath)
    {
        HtmlNode? birthsHeader = HtmlParsing.SelectNode(html, xpath);
        if (birthsHeader is null)
            return [];

        var sections = GetBirthSections(birthsHeader);
        return GetLiNodes(sections);
    }

    public string? ExtractPersonHref(string innerHtml)
    {
        var wrapper = HtmlNode.CreateNode("<div>" + innerHtml + "</div>");
        var anchors = wrapper.SelectNodes(".//a");

        if (anchors is { Count: >= 2 })
        {
            var href = anchors[1].GetAttributeValue("href", "");
            return string.IsNullOrWhiteSpace(href) ? null : href;
        }

        return null;
    }

    public string? ExtractHrefForEntry(HtmlNode li, string personName)
    {
        var anchors = li.SelectNodes(".//a");
        if (anchors == null || anchors.Count < 2)
            return null;

        foreach (var a in anchors.Skip(1))
        {
            string text = HtmlEntity.DeEntitize(a.InnerText).Trim();

            if (personName.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                text.Contains(personName, StringComparison.OrdinalIgnoreCase))
            {
                return a.GetAttributeValue("href", "");
            }
        }

        return null;
    }

    private static IEnumerable<HtmlNode> GetBirthSections(HtmlNode birthsHeader)
    {
        for (HtmlNode node = birthsHeader.NextSibling; node != null; node = node.NextSibling)
        {
            if (node.NodeType == HtmlNodeType.Text)
                continue;

            if (node.Name.Equals("h2", StringComparison.OrdinalIgnoreCase))
                yield break;

            var lis = node.SelectNodes(".//li");
            if (lis == null)
                continue;

            foreach (var li in lis)
                yield return li;
        }
    }

    private static List<HtmlNode> GetLiNodes(IEnumerable<HtmlNode> sections)
    {
        List<HtmlNode> results = [];

        foreach (var section in sections)
        {
            if (section.Name.Equals("li", StringComparison.OrdinalIgnoreCase))
            {
                results.Add(section);
                continue;
            }

            var liNodes = section.SelectNodes(".//li");
            if (liNodes != null)
                results.AddRange(liNodes);
        }

        return results;
    }
}
