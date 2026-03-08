using BirthdayCollator.Helpers;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Dates;
using BirthdayCollator.Server.Processing.Entries;
using BirthdayCollator.Server.Processing.Html;
using BirthdayCollator.Server.Processing.Links;
using BirthdayCollator.Server.Processing.Names;
using BirthdayCollator.Server.Processing.Pipelines;
using BirthdayCollator.Server.Processing.Validation;

namespace BirthdayCollator.Server.Processing.Parsers;

public sealed class WikiParser(
    IHtmlBirthSectionExtractor htmlExtractor,
    IEntrySplitter entrySplitter,
    ILinkResolver linkResolver,
    IBirthDateParser dateParser,       
    IPersonNameResolver nameResolver,
    Func<string, string> normalizeWikiHref
) : IWikiParser

{
    public List<Person> Parse(string html, DateTime birthDate, string? suffix, string xpath, bool includeAll)
    {
        var validator = new BirthEntryValidator([birthDate.Year.ToString()], RegexPatterns.ExcludeDiedRegex());
        var personFactory = new PersonFactory(normalizeWikiHref, nameResolver);
        var nodes = htmlExtractor.ExtractLiNodes(html, xpath);
        
        if (nodes.Count == 0) return [];

        List<Person> results = [];

        foreach (var node in nodes)
        {
            string entry = node.InnerText;

            if (!includeAll &&
                (entrySplitter.IsDeathEntry(entry) ||
                 !validator.IsValidBirthEntry(entry, birthDate.Year, node)))
                continue;

            if (!dateParser.MatchesRequestedDate(entry, birthDate))
                continue;

            var personLink = linkResolver.FindPersonLink(node, entry);
            if (personLink == null) continue;

            var person = personFactory.BuildPerson(entry, birthDate, personLink);

            person.SourceSlug = string.IsNullOrEmpty(suffix)
                ? $"{birthDate.Year}"
                : $"{birthDate.Year}_{suffix}";

            results.Add(personFactory.Finalize(person));
        }

        return results;
    }
}
