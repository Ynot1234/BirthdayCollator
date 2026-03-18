namespace BirthdayCollator.Server.Processing.Sources;

public sealed class ImdbSource(
    ImdbFetcher fetcher,
    ImdbParser parser,
    IYearRangeProvider yearRangeProvider) : IBirthSource
{
    public async Task<List<Person>> GetPeopleAsync(DateTime date, CancellationToken ct)
    {
        IReadOnlyList<string> targetYears = yearRangeProvider.GetYears();

        var tasks = targetYears
            .Select(yearStr => int.TryParse(yearStr, out int year)
                ? FetchAndParse(year, date.Month, date.Day, ct)
                : Task.FromResult(new List<Person>()));

        var results = await Task.WhenAll(tasks);

        List<Person> allPeople = new(results.Sum(r => r.Count));
        foreach (var list in results)
        {
            allPeople.AddRange(list);
        }

        if (date is { Month: 2, Day: 29 })
        {
            var leapYears = yearRangeProvider.GetLeapYears()
                .Select(int.Parse)
                .ToHashSet();

            return allPeople.Where(p => leapYears.Contains(p.BirthYear)).ToList();
        }

        return allPeople;
    }

    private async Task<List<Person>> FetchAndParse(int y, int m, int d, CancellationToken ct)
    {
        string html = await fetcher.FetchAsync(y, m, d, ct);
        return parser.Parse(html, y, m, d);
    }

    public bool IsRelevant(BirthSourceOptions opt, IYearRangeProvider years, DateTime date) => opt.EnableImdbParser;
}