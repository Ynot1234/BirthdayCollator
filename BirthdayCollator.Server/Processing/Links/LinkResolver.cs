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
        var links = li.SelectNodes(".//a");
        if (links is null || links.Count == 0) return null;

        string normalizedPerson = WikiTextUtility.ExtractPersonName(entry);
        var nonDateLinks = links.Where(a => !IsDateHref(a.GetAttributeValue("href", ""))).ToList();

        if (nonDateLinks.Count == 0) return null;

        HtmlNode? fuzzyMatch = null;
        foreach (var a in nonDateLinks)
        {
            string linkText = HtmlEntity.DeEntitize(a.InnerText).Trim();
            if (string.Equals(linkText, normalizedPerson, StringComparison.OrdinalIgnoreCase))
                return a;

            if (fuzzyMatch is null && (normalizedPerson.Contains(linkText, StringComparison.OrdinalIgnoreCase) ||
                                       linkText.Contains(normalizedPerson, StringComparison.OrdinalIgnoreCase)))
            {
                fuzzyMatch = a;
            }
        }

        return fuzzyMatch ?? nonDateLinks.FirstOrDefault();
    }

    public bool IsDateHref(string href)
    {
        if (string.IsNullOrWhiteSpace(href) || !href.StartsWith("./", StringComparison.Ordinal))
            return false;

        var parts = href[2..].Split('_', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 2 && DateTime.TryParse($"{parts[0]} 1", out _) && int.TryParse(parts[1], out _);
    }

    public bool HrefMatchesName(string href, string name)
    {
        if (string.IsNullOrWhiteSpace(href) || string.IsNullOrWhiteSpace(name)) return false;

        var hTokens = WikiTextUtility.Tokenize(WikiTextUtility.ToComparableSlug(href))
            .Except(NameParsing.Stopwords).Except(NameParsing.Titles).ToList();
        var nTokens = WikiTextUtility.Tokenize(WikiTextUtility.ToComparableSlug(name))
            .Except(NameParsing.Stopwords).Except(NameParsing.Titles).ToList();

        return nTokens.All(nt => hTokens.Any(ht => ht.Contains(nt) || nt.Contains(ht)));
    }

    public bool TryApplyHrefOverride(Person person, string? href)
    {
        if (string.IsNullOrWhiteSpace(href)) return true;

        person.Url = Urls.Domain + WikiUrlBuilder.NormalizeWikiHref(href);
        return HrefMatchesName(href, person.Name);
    }

    public string? ResolvePrimaryHref(string? innerHtml)
    {
        if (string.IsNullOrWhiteSpace(innerHtml)) return null;

        var href = HtmlNode.CreateNode($"<div>{innerHtml}</div>")
            .SelectNodes(".//a")?.Skip(1).FirstOrDefault()?.GetAttributeValue("href", "");

        return string.IsNullOrWhiteSpace(href) ? null : href;
    }
}
