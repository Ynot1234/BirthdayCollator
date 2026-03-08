using BirthdayCollator.Helpers;
using BirthdayCollator.Server.Configuration;
using BirthdayCollator.Server.Helpers;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Fetching;
using BirthdayCollator.Server.Processing.Names;
using BirthdayCollator.Server.Processing.Parsers;
using BirthdayCollator.Server.Processing.Validation;

namespace BirthdayCollator.Server.Processing.Sources;

public sealed class DateBirthSource(
    WikiHtmlFetcher fetcher,
    IYearRangeProvider yearRangeProvider,
    IPersonNameResolver nameResolver
) : IBirthSource
{
    private readonly IYearRangeProvider _yearRangeProvider = yearRangeProvider;

    public async Task<List<Person>> GetPeopleAsync(DateTime actualDate, CancellationToken token)
    {
            var people = new List<Person>();
            var years = _yearRangeProvider.GetYears();

            var validator = new BirthEntryValidator([.. years], RegexPatterns.ExcludeDiedRegex());
            var factory = new PersonFactory(WikiUrlBuilder.NormalizeWikiHref, nameResolver);
            var parser = new DatePageParser(validator, factory);
            var pageName = $"{actualDate:MMMM}_{actualDate.Day}";
           
            people.AddRange(await FetchAndParse(
                pageName,
                actualDate.Month,
                actualDate.Day,
                parser,
                token
            ));
        

        if (LeapYear.IsNonLeapFeb28(actualDate.Month, actualDate.Day))
        {
            var leapYears = _yearRangeProvider.GetLeapYears().ToHashSet();
            var leapValidator = new BirthEntryValidator([.. leapYears], RegexPatterns.ExcludeDiedRegex());
            var leapFactory = new PersonFactory(WikiUrlBuilder.NormalizeWikiHref, nameResolver);
            var leapParser = new DatePageParser(leapValidator, leapFactory);

            people.AddRange(await FetchAndParse(
                "February_29",
                actualDate.Month,
                actualDate.Day + 1,
                leapParser,
                token
            ));
        }

        return people;
    }

    private async Task<List<Person>> FetchAndParse(
        string pageName,
        int logicalMonth,
        int logicalDay,
        DatePageParser parser,
        CancellationToken token
    )
    {
        try
        {
            token.ThrowIfCancellationRequested();
            string html = await fetcher.FetchHtmlAsync(pageName, token);
            token.ThrowIfCancellationRequested();

            return parser.Parse(html, logicalMonth, logicalDay);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return [];
        }
    }

    public bool IsRelevant(BirthSourceOptions opt, IYearRangeProvider years, DateTime date) => opt.EnableDateParser;
}
