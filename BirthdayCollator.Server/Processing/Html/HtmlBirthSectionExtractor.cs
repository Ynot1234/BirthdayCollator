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
            if (node.NodeType == HtmlNodeType.Element &&
                node.Name.Length == 2 &&
                node.Name.StartsWith("h", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (node.NodeType == HtmlNodeType.Element)
            {
                if (node.Name is "ul" or "ol")
                {
                    var items = node.SelectNodes("./li");
                    if (items != null) results.AddRange(items);
                }
                else if (node.Name == "li")
                {
                    results.Add(node);
                }
                else
                {
                    var nestedItems = node.SelectNodes(".//li");
                    if (nestedItems != null) results.AddRange(nestedItems);
                }
            }
        }

        return results;
    }
}