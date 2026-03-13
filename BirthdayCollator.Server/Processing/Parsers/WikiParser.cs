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

        DateTime? activeDate = null;

        foreach (var node in nodes)
        {
            string entry = node.InnerText;

            if(node.InnerText.Contains("Awoonor"))
            {
                int r = 4;
            }
            
            if (dateParser.TryParseMonthDay(entry, birthDate.Year, out var parsedDate))
            {
                activeDate = parsedDate;
            }

            if (entrySplitter.IsDeathEntry(entry) || !validator.IsValidBirthEntry(entry, birthDate.Year, birthDate.Month, birthDate.Day, node))
                continue;

            bool isDateValid;

            if (includeAll)
            {
                var dateToCheck = activeDate ?? (dateParser.TryParseMonthDay(entry, birthDate.Year, out var d) ? d : null);
                isDateValid = dateToCheck.HasValue && dateToCheck.Value >= birthDate;
            }
            else
            {
                isDateValid = (activeDate.HasValue && activeDate.Value.Month == birthDate.Month && activeDate.Value.Day == birthDate.Day)
                              || dateParser.MatchesRequestedDate(entry, birthDate);
            }

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