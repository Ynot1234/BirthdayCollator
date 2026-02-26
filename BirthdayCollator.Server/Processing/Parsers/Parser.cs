using BirthdayCollator.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Dates;
using BirthdayCollator.Server.Processing.Entries;
using BirthdayCollator.Server.Processing.Html;
using BirthdayCollator.Server.Processing.Links;
using BirthdayCollator.Server.Processing.Validation;
using HtmlAgilityPack;

namespace BirthdayCollator.Server.Processing.Parsers;

public sealed partial class Parser(
    BirthEntryValidator validator,
    PersonFactory personFactory,
    IHtmlBirthSectionExtractor htmlExtractor,
    IBirthDateParser dateParser,
    IEntrySplitter entrySplitter,
    ILinkResolver linkResolver)
{
    private const bool AllForYear = false;

    public List<Person> Parse(string html, DateTime actualDate, string? suffix, string xpath)
    {
        var liNodes = htmlExtractor.ExtractLiNodes(html, xpath);
        if (liNodes.Count == 0)
            return [];

        List<Person> results = [];

        foreach (HtmlNode li in liNodes)
        {
            string raw = HtmlEntity.DeEntitize(li.InnerText).Trim();
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            string? dateHeader = dateParser.GetYearPageDateHeader(raw);
            if (string.IsNullOrWhiteSpace(dateHeader))
                continue;

            if (!dateParser.TryParseMonthDay(dateHeader, actualDate.Year, out DateTime parentDate))
                continue;

            if (!AllForYear && !dateParser.MatchesRequestedDate(dateHeader, actualDate))
                continue;

            string? href = htmlExtractor.ExtractPersonHref(li.InnerHtml);
            var ctx = new EntryContext(raw, href, entrySplitter.IsMulti(raw), parentDate);

            foreach (var (Text, Date) in entrySplitter.SplitEntries(ctx))
            {
                string? entryHref = htmlExtractor.ExtractHrefForEntry(li, Text);
                ProcessEntry(results, li, Text, ctx with { Href = entryHref }, Date, suffix);
            }
        }

        return results;
    }

    private void ProcessEntry(
        List<Person> results,
        HtmlNode li,
        string entry,
        EntryContext ctx,
        DateTime birthDate,
        string? suffix)
    {
        if (entrySplitter.IsDeathEntry(entry))
            return;

        if (!validator.IsValidBirthEntry(entry, birthDate.Year, li))
            return;

        var personLink = linkResolver.FindPersonLink(li, entry);
        if (personLink is null)
            return;

        Person person = personFactory.BuildPerson(entry, birthDate, personLink);
        person = personFactory.CreateWithSuffix(person, birthDate, suffix);
        person = personFactory.Finalize(person);

        if (!linkResolver.TryApplyHrefOverride(person, ctx.Href))
            return;

        results.Add(person);
    }
}
