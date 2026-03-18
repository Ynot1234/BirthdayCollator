namespace BirthdayCollator.Server.Processing.Links;

public interface ILinkResolver
{
    HtmlNode? FindPersonLink(HtmlNode li, string entry);
    string? ExtractWikipediaHref(HtmlNode node);
}

public sealed class LinkResolver : ILinkResolver
{
    public HtmlNode? FindPersonLink(HtmlNode li, string entry)
    {
        var allLinks = li.SelectNodes(".//a");
        if (allLinks == null || allLinks.Count == 0) return null;

        var candidates = new List<HtmlNode>(allLinks.Count);
        foreach (var link in allLinks)
        {
            var href = link.GetAttributeValue("href", string.Empty);
            if (!WikiValidator.IsDateHref(href))
            {
                candidates.Add(link);
            }
        }

        if (candidates.Count == 0) return null;

        string normalized = WikiTextUtility.ExtractPersonName(entry);

        foreach (var c in candidates)
        {
            string innerText = HtmlEntity.DeEntitize(c.InnerText).Trim();
            if (string.Equals(innerText, normalized, StringComparison.OrdinalIgnoreCase))
            {
                return c;
            }
        }

        foreach (var c in candidates)
        {
            string innerText = HtmlEntity.DeEntitize(c.InnerText).Trim();
            if (WikiTextUtility.FuzzyNameMatch(normalized, innerText))
            {
                return c;
            }
        }

        return candidates[0];
    }

    public string? ExtractWikipediaHref(HtmlNode node)
    {
        var link = node.Name == "a" ? node : node.SelectSingleNode(".//a");

        if (link == null) return null;

        var url = link.GetAttributeValue("href", string.Empty);

        if (string.IsNullOrWhiteSpace(url) || !url.Contains("wikipedia.org", StringComparison.OrdinalIgnoreCase))
            return null;

        return WikiUrlBuilder.NormalizeWikiHref(url);
    }
}