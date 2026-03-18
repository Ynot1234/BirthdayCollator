namespace BirthdayCollator.Server.Processing.Html;

public static class WikipediaDomNavigator
{
    public static List<HtmlNode> ExtractBirthLiNodes(HtmlDocument htmlDoc)
    {
        HtmlNode birthsHeader = htmlDoc.DocumentNode.SelectSingleNode(XPathSelectors.YearBirthsHeader);
        if (birthsHeader == null) return [];

        List<HtmlNode> results = [];

        for (HtmlNode node = birthsHeader.NextSibling; node != null; node = node.NextSibling)
        {
            if (node.NodeType == HtmlNodeType.Element &&
                node.Name.Equals("h2", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (node.NodeType == HtmlNodeType.Element)
            {
                if (node.Name == "ul")
                {
                    var items = node.SelectNodes("./li");
                    if (items != null) results.AddRange(items);
                }
                else if (node.Name is "section" or "div")
                {
                    var items = node.SelectNodes(".//li");
                    if (items != null) results.AddRange(items);
                }
            }
        }

        return results;
    }
}