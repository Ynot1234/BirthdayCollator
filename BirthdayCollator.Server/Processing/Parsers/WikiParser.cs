using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Dates;
using BirthdayCollator.Server.Processing.Entries;
using BirthdayCollator.Server.Processing.Html;
using BirthdayCollator.Server.Processing.Links;
using BirthdayCollator.Server.Processing.Pipelines;
using BirthdayCollator.Server.Processing.Validation;

namespace BirthdayCollator.Server.Processing.Parsers;
public sealed class WikiParser(IHtmlBirthSectionExtractor htmlExtractor, IEntrySplitter entrySplitter, ILinkResolver linkResolver,
                               IBirthDateParser dateParser, BirthEntryValidator validator, PersonFactory personFactory) : IWikiParser
{
    public List<Person> Parse(string html, DateTime birthDate, string? suffix, string xpath, bool includeAll)
    {
        var nodes = htmlExtractor.ExtractLiNodes(html, xpath);
        if (nodes.Count == 0) return [];
        List<Person> results = [];
        string sourceSlug = string.IsNullOrEmpty(suffix) ? $"{birthDate.Year}" : $"{birthDate.Year}_{suffix}";

        foreach (var node in nodes)
        {
            string entry = node.InnerText;

            if (entrySplitter.IsDeathEntry(entry) || !validator.IsValidBirthEntry(entry, birthDate.Year, birthDate.Month, birthDate.Day, node))
                continue;

            bool isDateValid = includeAll ? dateParser.IsOnOrAfterDate(entry, birthDate) : dateParser.MatchesRequestedDate(entry, birthDate);
            if (!isDateValid) continue;

            var personLink = linkResolver.FindPersonLink(node, entry);
            if (personLink == null) continue;

            string name = WikiTextUtility.Normalize(personLink.InnerText);
            string description = WikiTextUtility.SanitizeWikiText(entry);
            string rawHref = personLink.GetAttributeValue("href", string.Empty);
            string slug = rawHref.Split('/').Last().TrimStart('.');
            string absoluteUrl = !string.IsNullOrEmpty(slug) ? $"{Urls.ArticleBase}/{slug}" : string.Empty;

            results.Add(personFactory.CreatePerson(
                name: name,
                desc: description,
                birthDate: birthDate,
                url: absoluteUrl,
                sourceSlug: sourceSlug,
                section: AppStrings.Sections.Births,
                displaySlug: slug
            ));
        }
        return results;
    }
}