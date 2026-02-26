using BirthdayCollator.Constants;
using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using HtmlAgilityPack;
using System.Globalization;
using System.Text;

namespace BirthdayCollator.Server.Processing.Links;

public interface ILinkResolver
{
    HtmlNode? FindPersonLink(HtmlNode li, string entry);
    bool IsDateHref(string href);
    bool HrefMatchesName(string href, string name);

    bool TryApplyHrefOverride(Person person, string? href);
}



public sealed class LinkResolver : ILinkResolver
{
    public HtmlNode? FindPersonLink(HtmlNode li, string entry)
    {
        var links = li.SelectNodes(".//a");
        if (links is null || links.Count == 0)
            return null;

        string personName = ExtractPersonName(entry);
        string normalizedPerson = HtmlEntity.DeEntitize(personName).Trim();

        List<HtmlNode> nonDateLinks = links
            .Where(a => !IsDateHref(a.GetAttributeValue("href", "")))
            .ToList();

        if (nonDateLinks.Count == 0)
            return null;

        foreach (var a in nonDateLinks)
        {
            string linkText = HtmlEntity.DeEntitize(a.InnerText).Trim();
            if (string.Equals(linkText, normalizedPerson, StringComparison.OrdinalIgnoreCase))
                return a;
        }

        foreach (var a in nonDateLinks)
        {
            string linkText = HtmlEntity.DeEntitize(a.InnerText).Trim();
            bool personContainsLink = normalizedPerson.Contains(linkText, StringComparison.OrdinalIgnoreCase);
            bool linkContainsPerson = linkText.Contains(normalizedPerson, StringComparison.OrdinalIgnoreCase);

            if (personContainsLink || linkContainsPerson)
                return a;
        }

        return nonDateLinks.FirstOrDefault();
    }

    public bool IsDateHref(string href)
    {
        if (string.IsNullOrWhiteSpace(href))
            return false;

        if (!href.StartsWith("./", StringComparison.Ordinal))
            return false;

        string rest = href[2..];
        var parts = rest.Split('_', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
            return false;

        string month = parts[0];
        string day = parts[1];

        if (!DateTime.TryParse($"{month} 1", out _))
            return false;

        return int.TryParse(day, out _);
    }

    public bool HrefMatchesName(string href, string name)
    {
        if (string.IsNullOrWhiteSpace(href) || string.IsNullOrWhiteSpace(name))
            return false;

        href = Normalize(href);
        name = Normalize(name);

        List<string> hrefTokens = Tokenize(href);
        List<string> nameTokens = Tokenize(name);

        hrefTokens = hrefTokens.Except(NameParsing.Stopwords).Except(NameParsing.Titles).ToList();
        nameTokens = nameTokens.Except(NameParsing.Stopwords).Except(NameParsing.Titles).ToList();

        foreach (string nt in nameTokens)
        {
            bool tokenMatches = hrefTokens.Any(ht => ht.Contains(nt) || nt.Contains(ht));
            if (!tokenMatches)
                return false;
        }

        return true;
    }

    private static string ExtractPersonName(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var idx = text.IndexOf('(');
        if (idx > 0)
            text = text[..idx];

        return text.Trim().TrimEnd(',');
    }

    private static string Normalize(string s)
    {
        s = s.ToLowerInvariant().Replace("_", " ");
        s = s.Normalize(NormalizationForm.FormD);
        var chars = s.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
        return new string(chars.ToArray());
    }

    private static List<string> Tokenize(string s)
    {
        return [.. s
            .Split([' ', ',', '.', '/', '-', '(', ')', '\''], StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .Where(t => t.Length > 0)];
    }

    public bool TryApplyHrefOverride(Person person, string? href)
    {
        if (string.IsNullOrWhiteSpace(href))
            return true;

        string slug = WikiUrlBuilder.NormalizeWikiHref(href);
        person.Url = Urls.Domain + slug;

        return HrefMatchesName(href, person.Name);
    }


}
