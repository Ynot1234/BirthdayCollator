using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Html;
using HtmlAgilityPack;

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
        var links = li.SelectNodes(".//a")?.ToList() ?? [];
        if (links.Count == 0) return null;

        List<HtmlNode> candidates = [.. links.Where(a => !WikiValidator.IsDateHref(a.GetAttributeValue("href", "")))];
        if (candidates.Count == 0) return null;

        string normalized = WikiTextUtility.ExtractPersonName(entry);

        var exact = candidates.FirstOrDefault(c =>
            string.Equals(HtmlEntity.DeEntitize(c.InnerText).Trim(), normalized, StringComparison.OrdinalIgnoreCase));

        if (exact != null) return exact;

        HtmlNode? fuzzy = candidates.FirstOrDefault(c => WikiTextUtility.FuzzyNameMatch(normalized, HtmlEntity.DeEntitize(c.InnerText).Trim()));

        return fuzzy ?? candidates.First();
    }

    public string? ExtractWikipediaHref(HtmlNode node)
    {
        var url = node.SelectSingleNode(".//a[contains(@href,'wikipedia.org')]")?.GetAttributeValue("href", "");
        return string.IsNullOrWhiteSpace(url) ? null : WikiUrlBuilder.NormalizeWikiHref(url);
    }
}