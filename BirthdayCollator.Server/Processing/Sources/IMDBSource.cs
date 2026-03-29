namespace BirthdayCollator.Server.Processing.Sources;

public sealed class ImdbSource(
    ImdbFetcher fetcher,
    ImdbParser parser,
    IYearRangeProvider yearRangeProvider) : IBirthSource
{
    public async Task<List<Person>> GetPeopleAsync(DateTime date, CancellationToken ct)
    {
        int currentYear = DateTime.Today.Year;
        int[] milestones = [80, 70, 60];

        List<(int Start, int End)> batches =
        [
            (currentYear - 105, currentYear - 85),
        .. milestones.Select(age => (currentYear - age, currentYear - age))
        ];

        var tasks = batches.Select(range =>
            FetchRangeAndParse(range.Start, range.End, date.Month, date.Day, ct));

        var results = await Task.WhenAll(tasks);
        List<Person> allPeople = [.. results.SelectMany(p => p)];

        if (date is { Month: 2, Day: 29 })
        {
            HashSet<int> leapYears = [.. yearRangeProvider.GetLeapYears()
            .Select(y => int.TryParse(y, out int year) ? year : 0)
            .Where(y => y > 0)];

            return [.. allPeople.Where(p => leapYears.Contains(p.BirthYear))];
        }

        return allPeople;
    }

    private async Task<List<Person>> FetchRangeAndParse(int startYear, int endYear, int m, int d, CancellationToken ct)
    {
        string urlRange = $"{startYear},{endYear}";
        string html = await fetcher.FetchRangeAsync(urlRange, m, d, ct);
        return parser.Parse(html, m, d, startYear, endYear);
    }

    public bool IsRelevant(BirthSourceOptions opt, IYearRangeProvider years, DateTime date) => opt.EnableImdbParser;
}