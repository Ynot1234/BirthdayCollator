using BirthdayCollator.Server.Processing.Parsers;

namespace BirthdayCollator.Server.Processing.Sources;

public sealed class DateBirthSource(WikiHtmlFetcher fetcher, IYearRangeProvider years, IDatePageParser parser) : IBirthSource
{
    public async Task<List<Person>> GetPeopleAsync(DateTime date, CancellationToken ct)
    {
        var people = await FetchAndParse($"{date:MMMM_d}", date.Month, date.Day, ct);

        if (date is { Month: 2, Day: 29 })
        {
            var leapYears = years.GetLeapYears().ToHashSet();
            return [.. people.Where(p => leapYears.Contains(p.BirthYear.ToString()))];
        }

        return people;
    }

    private async Task<List<Person>> FetchAndParse(string page, int m, int d, CancellationToken ct)
    {
        string html = await fetcher.FetchHtmlAsync(page, ct);
        return parser.Parse(html, m, d);
    }

    public bool IsRelevant(BirthSourceOptions opt, IYearRangeProvider y, DateTime d) => opt.EnableDateParser;
}