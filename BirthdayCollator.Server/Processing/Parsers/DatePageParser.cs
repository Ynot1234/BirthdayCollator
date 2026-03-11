using HtmlAgilityPack;
using System.Globalization;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Validation;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Html;
using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Processing.Links;

namespace BirthdayCollator.Server.Processing.Parsers;

public sealed partial class DatePageParser( BirthEntryValidator validator, PersonFactory personFactory, ILinkResolver linkResolver) : IDatePageParser
{
    public List<Person> Parse(string html, int month, int day)
    {
        HtmlDocument htmlDoc = new();
        htmlDoc.LoadHtml(html);

        var liNodes = WikipediaDomNavigator.ExtractBirthLiNodes(htmlDoc);
        if (liNodes.Count == 0) return [];

        List<Person> results = [];
        string sourceSlug = $"{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month)}_{day}";

        foreach (var li in liNodes)
        {
            string entry = WikiTextUtility.Normalize(li.InnerText);

            if (!entry.Contains('–') || !WikiTextUtility.TryExtractBirthYear(entry, out int birthYear)) continue;
            if (!validator.IsValidBirthEntry(entry, birthYear, month, day, li)) continue;

            var link = linkResolver.FindPersonLink(li, entry);
            if (link == null) continue;

            string name = !string.IsNullOrWhiteSpace(link.InnerText)
                ? WikiTextUtility.Normalize(link.InnerText)
                : WikiTextUtility.ExtractPersonName(entry);

            string description = WikiTextUtility.SanitizeWikiText(entry);
            string rawHref = link.GetAttributeValue("href", string.Empty);
            string slug = rawHref.Split('/').Last().TrimStart('.');
            string absoluteUrl = !string.IsNullOrEmpty(slug) ? $"{Urls.ArticleBase}/{slug}" : string.Empty;

            results.Add(personFactory.CreatePerson(
                name: name,
                desc: description,
                birthDate: new DateTime(birthYear, month, day),
                url: absoluteUrl,
                sourceSlug: sourceSlug,
                section: AppStrings.Sections.Births,
                displaySlug: slug
            ));
        }
        return results;
    }
}