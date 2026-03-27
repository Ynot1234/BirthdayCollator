using BirthdayCollator.Server.Processing.Dates;
using BirthdayCollator.Server.Processing.Entries;

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
            // Split the InnerText into lines to handle multi-person blocks
            var lines = node.InnerText.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (string line in lines)
            {
                if (dateParser.TryParseMonthDay(line, birthDate.Year, out var parsedDate))
                {
                    activeDate = parsedDate;

                    bool isJustADateHeader = !line.Contains('–') && !line.Contains(',') && !line.Contains('-');

                    if (isJustADateHeader)
                    {
                        continue;
                    }
                }

                bool isDeceased = line.Contains("died", StringComparison.OrdinalIgnoreCase) ||  entrySplitter.IsDeathEntry(line);

                if (isDeceased) continue;

                if (!validator.IsValidBirthEntry(line, birthDate.Year, birthDate.Month, birthDate.Day, node))
                    continue;

                if (!IsDateValid(line, birthDate, activeDate, includeAll))
                    continue;

                var personLink = linkResolver.FindPersonLink(node, line);
                if (personLink is null) continue;

                string rawHref = personLink.GetAttributeValue("href", string.Empty);
                string slug = rawHref.TrimStart('.', '/');
                string absoluteUrl = !string.IsNullOrEmpty(slug) ? $"{Urls.ArticleBase}/{slug}" : string.Empty;

                results.Add(personFactory.CreatePerson(
                    name: WikiTextUtility.Normalize(personLink.InnerText),
                    desc: WikiTextUtility.SanitizeWikiText(line),
                    birthDate: birthDate,
                    url: absoluteUrl,
                    sourceSlug: sourceSlug,
                    section: Sections.Births,
                    displaySlug: slug
                ));
            }
        }
        return results;
    }
    private bool IsDateValid(string line, DateTime birthDate, DateTime? activeDate, bool includeAll)
    {
        if (includeAll)
        {
            var dateToCheck = activeDate ?? (dateParser.TryParseMonthDay(line, birthDate.Year, out var d) ? d : null);
            return dateToCheck.HasValue && dateToCheck.Value >= birthDate;
        }

        return (activeDate.HasValue && activeDate.Value.Month == birthDate.Month && activeDate.Value.Day == birthDate.Day)
               || dateParser.MatchesRequestedDate(line, birthDate);
    }
}