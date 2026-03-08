using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Html;
using HtmlAgilityPack;

namespace BirthdayCollator.Server.Processing.Links;

public interface ILinkResolver
{
    HtmlNode? FindPersonLink(HtmlNode li, string entry);
    bool IsDateHref(string href);
    bool HrefMatchesName(string href, string name);
    bool TryApplyHrefOverride(Person person, string? href);
    string? ResolvePrimaryHref(string? innerHtml);
}

public sealed class LinkResolver : ILinkResolver
{
    public HtmlNode? FindPersonLink(HtmlNode li, string entry)
    {
        var links = li.SelectNodes(".//a")?.ToList() ?? [];
        string normalized = WikiTextUtility.ExtractPersonName(entry);

        var candidates = links
            .Where(a => !IsDateHref(a.GetAttributeValue("href", "")))
            .Select(a => new
            {
                Node = a,
                Text = HtmlEntity.DeEntitize(a.InnerText).Trim()
            })
            .ToList();

        if (candidates.Count == 0)
            return null;

        var exact = candidates
            .FirstOrDefault(c => string.Equals(c.Text, normalized, StringComparison.OrdinalIgnoreCase));

        if (exact is not null)
            return exact.Node;

        var fuzzy = candidates
            .FirstOrDefault(c => WikiTextUtility.FuzzyNameMatch(normalized, c.Text));

        return fuzzy?.Node ?? candidates.First().Node;
    }

    public bool IsDateHref(string href)
    {
        if (string.IsNullOrWhiteSpace(href) || !href.StartsWith("./", StringComparison.Ordinal))
            return false;

        var parts = href[2..].Split('_', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 2 &&
               DateTime.TryParse($"{parts[0]} 1", out _) &&
               int.TryParse(parts[1], out _);
    }

    public bool HrefMatchesName(string href, string name)
    {
        var hTokens = WikiTextUtility.Tokenize(WikiTextUtility.ToComparableSlug(href))
            .Except(NameParsing.Stopwords)
            .Except(NameParsing.Titles)
            .ToList();

        var nTokens = WikiTextUtility.Tokenize(WikiTextUtility.ToComparableSlug(name))
            .Except(NameParsing.Stopwords)
            .Except(NameParsing.Titles)
            .ToList();

        return nTokens.All(nt => hTokens.Any(ht =>
            ht.Contains(nt, StringComparison.OrdinalIgnoreCase) ||
            nt.Contains(ht, StringComparison.OrdinalIgnoreCase)));
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


    public bool TryApplyHrefOverride(Person person, string? href)
    {
        if (string.IsNullOrWhiteSpace(href))
            return true;

        person.Url = Urls.Domain + WikiUrlBuilder.NormalizeWikiHref(href);
        return HrefMatchesName(href, person.Name);
    }

    public static string? ExtractWikipediaHref(HtmlNode node)
    {
        var url = node.SelectSingleNode(".//a[contains(@href,'wikipedia.org')]")
                      ?.GetAttributeValue("href", "");

        return string.IsNullOrWhiteSpace(url) ? null : WikiUrlBuilder.NormalizeWikiHref(url);
    }

    public string? ResolvePrimaryHref(string? innerHtml)
    {
        if(string.IsNullOrWhiteSpace(innerHtml))
            return null;

        return HtmlNode.CreateNode($"<div>{innerHtml}</div>")
                .SelectNodes(".//a")?
                .Skip(1)
                .FirstOrDefault()?
                .GetAttributeValue("href", "");
    }
}