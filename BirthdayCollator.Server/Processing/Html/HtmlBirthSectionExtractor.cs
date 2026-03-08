using HtmlAgilityPack;

namespace BirthdayCollator.Server.Processing.Html;

public interface IHtmlBirthSectionExtractor
{
    List<HtmlNode> ExtractLiNodes(string html, string xpath);
}

public sealed class HtmlBirthSectionExtractor : IHtmlBirthSectionExtractor
{
    public List<HtmlNode> ExtractLiNodes(string html, string xpath)
    {
        if (string.IsNullOrWhiteSpace(html)) return [];

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var header = doc.DocumentNode.SelectSingleNode(xpath);
        if (header == null) return [];

        List<HtmlNode> results = [];

        for (var node = header.NextSibling; node != null; node = node.NextSibling)
        {
            if (node.Name.StartsWith("h", StringComparison.OrdinalIgnoreCase) && node.Name.Length == 2)
                break;

            var lis = node.SelectNodes(".//li");
            if (lis != null) results.AddRange(lis);
        }

        return results;
    }
}
