using BirthdayCollator.Server.Configuration;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Fetching;
using BirthdayCollator.Server.Processing.Parsers;

namespace BirthdayCollator.Server.Processing.Sources;

public sealed class ImdbSource(ImdbFetcher fetcher, ImdbParser parser, IYearRangeProvider yearRangeProvider) : IBirthSource
{
    public async Task<List<Person>> GetPeopleAsync(DateTime date, CancellationToken ct)
    {
        var targetYears = yearRangeProvider.GetYears();

        var tasks = targetYears.Select(yearStr =>
            int.TryParse(yearStr, out int year)
                ? FetchAndParse(year, date.Month, date.Day, ct)
                : Task.FromResult(new List<Person>()));

        var results = await Task.WhenAll(tasks);
        var allPeople = results.SelectMany(p => p).ToList();

        if (date is { Month: 2, Day: 29 })
        {
            var leapYears = yearRangeProvider.GetLeapYears().ToHashSet();
            return [.. allPeople.Where(p => leapYears.Contains(p.BirthYear.ToString()))];
        }

        return allPeople;
    }

    private async Task<List<Person>> FetchAndParse(int y, int m, int d, CancellationToken ct)
    {
        string html = await fetcher.FetchAsync(y, m, d, ct);
        return parser.Parse(html, y, m, d);
    }

    public bool IsRelevant(BirthSourceOptions opt, IYearRangeProvider years, DateTime date)  => opt.EnableImdbParser;
}