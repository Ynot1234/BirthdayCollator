using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Html;
using HtmlAgilityPack;

namespace BirthdayCollator.Server.Processing.Links;


public interface ILinkResolver
{
    HtmlNode? FindPersonLink(HtmlNode li, string entry);

    bool HrefMatchesName(string href, string name);
    bool TryApplyHrefOverride(Person person, string? href);

    string? ResolvePrimaryHref(string? innerHtml); 
    string? ExtractWikipediaHref(HtmlNode node);    

    bool IsDateHref(string href);
}

public sealed class LinkResolver : ILinkResolver
{
    public HtmlNode? FindPersonLink(HtmlNode li, string entry)
    {
        var links = li.SelectNodes(".//a")?.ToList() ?? [];

        if (links.Count == 0) return null;

        List<HtmlNode> candidates = [.. links.Where(a => !IsDateHref(a.GetAttributeValue("href", "")))];

        if (candidates.Count == 0) return null;

        string normalized = WikiTextUtility.ExtractPersonName(entry);

        var exact = candidates.FirstOrDefault(c =>
            string.Equals(HtmlEntity.DeEntitize(c.InnerText).Trim(), normalized, StringComparison.OrdinalIgnoreCase));

        if (exact != null) return exact;

        var fuzzy = candidates.FirstOrDefault(c =>
            WikiTextUtility.FuzzyNameMatch(normalized, HtmlEntity.DeEntitize(c.InnerText).Trim()));

        return fuzzy ?? candidates.First();
    }

    public bool IsDateHref(string href)
    {
        if (string.IsNullOrWhiteSpace(href)) return false;
        string trimmed = href.TrimStart('.', '/');

        if (trimmed.Length == 4 && int.TryParse(trimmed, out _)) return true;

        var parts = trimmed.Split('_');
        return parts.Length == 2 && DateTime.TryParse($"{parts[0]} 1", out _) && int.TryParse(parts[1], out _);
    }

    public string? ResolvePrimaryHref(string? innerHtml)
    {
        if (string.IsNullOrWhiteSpace(innerHtml)) return null;
        var href = HtmlNode.CreateNode($"<div>{innerHtml}</div>")
            .SelectNodes(".//a")?.Skip(1).FirstOrDefault()?.GetAttributeValue("href", "");

        return string.IsNullOrWhiteSpace(href) ? null : WikiUrlBuilder.NormalizeWikiHref(href);
    }

    public string? ExtractWikipediaHref(HtmlNode node)
    {
        var url = node.SelectSingleNode(".//a[contains(@href,'wikipedia.org')]")?.GetAttributeValue("href", "");
        return string.IsNullOrWhiteSpace(url) ? null : WikiUrlBuilder.NormalizeWikiHref(url);
    }

    public bool TryApplyHrefOverride(Person person, string? href)
    {
        if (string.IsNullOrWhiteSpace(href)) return true;
        person.Url = Urls.Domain + WikiUrlBuilder.NormalizeWikiHref(href);
        return HrefMatchesName(href, person.Name);
    }

    public bool HrefMatchesName(string href, string name) =>
        WikiTextUtility.FuzzyNameMatch(href, name); 
}