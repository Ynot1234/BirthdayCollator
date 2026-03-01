using BirthdayCollator.Server.Helpers;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Fetching;
using BirthdayCollator.Server.Processing.Parsers;

namespace BirthdayCollator.Server.Processing.Sources;


public sealed class OnThisDaySource(
    OnThisDayHtmlFetcher fetcher,
    OnThisDayParser parser,
    IYearRangeProvider yearRangeProvider
) : IBirthSource
{
    private readonly IYearRangeProvider _yearRangeProvider = yearRangeProvider;

    public async Task<List<Person>> GetPeopleAsync(DateTime actualDate, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        List<Person> people = [];
        string html = await fetcher.FetchAsync(actualDate.Month, actualDate.Day, token);
        var parsed = parser.Parse(html, actualDate.Month, actualDate.Day);
        people.AddRange(parsed);

        if (LeapYear.IsNonLeapFeb28(actualDate.Month, actualDate.Day))
        {
            html = await fetcher.FetchAsync(actualDate.Month, actualDate.Day + 1, token);
            parsed = parser.Parse(html, actualDate.Month, actualDate.Day + 1);
            people.AddRange(parsed);
        }

        IReadOnlySet<string> allowedYears = _yearRangeProvider.GetYearSet();

        return [.. people.Where(p => allowedYears.Contains(p.BirthYear.ToString()))];
    }
}