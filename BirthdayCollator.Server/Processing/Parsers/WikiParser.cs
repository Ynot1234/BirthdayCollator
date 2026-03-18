using BirthdayCollator.Server.Processing.Dates;
using BirthdayCollator.Server.Processing.Entries;
using BirthdayCollator.Server.Processing.Validation;

namespace BirthdayCollator.Server.Processing.Parsers;

public sealed class WikiParser(
    IHtmlBirthSectionExtractor htmlExtractor,
    IEntrySplitter entrySplitter,
    ILinkResolver linkResolver,
    IBirthDateParser dateParser,
    BirthEntryValidator validator,
    PersonFactory personFactory) : IWikiParser
{
    public List<Person> Parse(string html, DateTime birthDate, string? suffix, string xpath, bool includeAll)
    {
        var nodes = htmlExtractor.ExtractLiNodes(html, xpath);
      
        if (nodes.Count is 0) return [];

        List<Person> results = [];
        string sourceSlug = string.IsNullOrEmpty(suffix) ? $"{birthDate.Year}" : $"{birthDate.Year}_{suffix}";

        DateTime? activeDate = null;

        foreach (var node in nodes)
        {
            string entry = node.InnerText;

            if (dateParser.TryParseMonthDay(entry, birthDate.Year, out var parsedDate))
            {
                activeDate = parsedDate;
            }

            if (entrySplitter.IsDeathEntry(entry) || !validator.IsValidBirthEntry(entry, birthDate.Year, birthDate.Month, birthDate.Day, node))
                continue;

            if (!IsDateValid(entry, birthDate, activeDate, includeAll))
                continue;

            var personLink = linkResolver.FindPersonLink(node, entry);
            if (personLink is null) continue;

            string rawHref = personLink.GetAttributeValue("href", string.Empty);
            ReadOnlySpan<char> hrefSpan = rawHref.AsSpan();
            int lastSlash = hrefSpan.LastIndexOf('/');

            string slug = lastSlash == -1
                ? rawHref.TrimStart('.')
                : new string(hrefSpan[(lastSlash + 1)..].TrimStart('.'));

            string absoluteUrl = !string.IsNullOrEmpty(slug) ? $"{Urls.ArticleBase}/{slug}" : string.Empty;

            results.Add(personFactory.CreatePerson(
                name: WikiTextUtility.Normalize(personLink.InnerText),
                desc: WikiTextUtility.SanitizeWikiText(entry),
                birthDate: birthDate,
                url: absoluteUrl,
                sourceSlug: sourceSlug,
                section: Sections.Births,
                displaySlug: slug
            ));
        }
        return results;
    }

    private bool IsDateValid(string entry, DateTime birthDate, DateTime? activeDate, bool includeAll)
    {
        if (includeAll)
        {
            var dateToCheck = activeDate ?? (dateParser.TryParseMonthDay(entry, birthDate.Year, out var d) ? d : null);
            return dateToCheck.HasValue && dateToCheck.Value >= birthDate;
        }

        return (activeDate.HasValue && activeDate.Value.Month == birthDate.Month && activeDate.Value.Day == birthDate.Day)
               || dateParser.MatchesRequestedDate(entry, birthDate);
    }
}