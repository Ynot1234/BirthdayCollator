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

        string html;

        try
        {
            html = await fetcher.FetchAsync(actualDate.Month, actualDate.Day, token);
        }
        catch
        {
            return [];
        }

        List<Person> people = parser.Parse(html, actualDate.Month, actualDate.Day);

        IReadOnlySet<string> allowedYears = _yearRangeProvider.GetYearSet();

        List<Person> filtered = [.. people.Where(p => allowedYears.Contains(p.BirthYear.ToString()))];

        return filtered;
    }
}