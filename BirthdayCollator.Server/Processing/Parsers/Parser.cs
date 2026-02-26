using BirthdayCollator.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Dates;
using BirthdayCollator.Server.Processing.Entries;
using BirthdayCollator.Server.Processing.Html;
using BirthdayCollator.Server.Processing.Links;
using BirthdayCollator.Server.Processing.Validation;
using HtmlAgilityPack;

namespace BirthdayCollator.Server.Processing.Parsers
{
    public partial class Parser(
        BirthEntryValidator validator,
        PersonFactory personFactory,
        IHtmlBirthSectionExtractor htmlExtractor,
        IBirthDateParser dateParser,
        IEntrySplitter entrySplitter,
        ILinkResolver linkResolver)
    {
        // Manual test flag: set to true only when testing year pages.
        private const bool AllforYear = false;

        public List<Person> Parse(string html, DateTime actualDate, string? suffix, string xpath)
        {
            var liNodes = htmlExtractor.ExtractLiNodes(html, xpath);
            if (liNodes.Count == 0)
                return [];

            List<Person> results = [];

            foreach (HtmlNode li in liNodes)
            {
                string raw = HtmlEntity.DeEntitize(li.InnerText).Trim();
                bool isMulti = entrySplitter.IsMulti(raw);

                string dateHeaderRaw = raw;
                string? dateHeader = dateParser.GetYearPageDateHeader(dateHeaderRaw);

                if (string.IsNullOrWhiteSpace(dateHeader))
                    continue;

                if (!dateParser.TryParseMonthDay(dateHeader, actualDate.Year, out DateTime parentDate))
                    continue;

                string? href = htmlExtractor.ExtractPersonHref(li.InnerHtml);

                EntryContext ctx = new(raw, href, isMulti, parentDate);

                if (!AllforYear && !dateParser.MatchesRequestedDate(dateHeader, actualDate))
                    continue;

                var entries = entrySplitter.SplitEntries(ctx);

                foreach (var (Text, Date) in entries)
                {
                    string? entryHref = htmlExtractor.ExtractHrefForEntry(li, Text);

                    ProcessEntry(
                                 results,
                                 li,
                                 Text,
                                 ctx with { Href = entryHref },
                                 Date,
                                 suffix);

                }
            }

            return results;
        }

        public void ProcessEntry(
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

            Person parsed = personFactory.BuildPerson(entry, birthDate, personLink);

            Person person = personFactory.CreateWithSuffix(parsed, birthDate, suffix);

            person = personFactory.Finalize(person);



            if (!linkResolver.TryApplyHrefOverride(person, ctx.Href))
                return;

            results.Add(person);
        }
    }
}
