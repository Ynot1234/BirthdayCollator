using BirthdayCollator.Helpers;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Fetching;
using BirthdayCollator.Server.Processing.Names;
using BirthdayCollator.Server.Processing.Parsers;
using BirthdayCollator.Server.Processing.Validation;
using BirthdayCollator.Server.Helpers;

namespace BirthdayCollator.Server.Processing.Sources;

public sealed class DateBirthSource(WikiHtmlFetcher fetcher, 
                                    IYearRangeProvider yearRangeProvider, 
                                    IPersonNameResolver nameResolver) : IBirthSource
{
    private readonly IYearRangeProvider _yearRangeProvider = yearRangeProvider;

    public async Task<List<Person>> GetPeopleAsync(DateTime actualDate, CancellationToken token)
    {
        List<Person> people = [];

        {
            var yearSet = _yearRangeProvider.GetYearSet();
            var validator = new BirthEntryValidator([.. yearSet], RegexPatterns.ExcludeDiedRegex());
            var factory = new PersonFactory(WikiUrlBuilder.NormalizeWikiHref, nameResolver);
            var parser = new DatePageParser(validator, factory);

            string pageName = $"{actualDate:MMMM}_{actualDate.Day}";
            people.AddRange(await FetchAndParse(pageName, actualDate.Month, actualDate.Day, parser, token));
        }

        if (LeapYear.IsNonLeapFeb28(actualDate.Month, actualDate.Day))
        {
            var leapYearSet = _yearRangeProvider.GetLeapYears().ToHashSet();
            var leapValidator = new BirthEntryValidator([.. leapYearSet], RegexPatterns.ExcludeDiedRegex());
            var leapFactory = new PersonFactory(WikiUrlBuilder.NormalizeWikiHref, nameResolver);
            var leapParser = new DatePageParser(leapValidator, leapFactory);

            people.AddRange(await FetchAndParse("February_29", actualDate.Month, actualDate.Day + 1, leapParser, token));
        }

        return people;
    }

    private async Task<List<Person>> FetchAndParse(
        string pageName,
        int logicalMonth,
        int logicalDay,
        DatePageParser parser,
        CancellationToken token)
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



}