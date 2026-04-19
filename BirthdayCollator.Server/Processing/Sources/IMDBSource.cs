namespace BirthdayCollator.Server.Processing.Sources;

public sealed class ImdbSource(
    ImdbFetcher fetcher,
    ImdbParser parser,
    IYearRangeProvider yearRangeProvider) : IBirthSource
{
    public async Task<List<Person>> GetPeopleAsync(DateTime date, CancellationToken ct)
    {
        int currentYear = DateTime.Today.Year;

        List<int> overrideYears =
                   [.. yearRangeProvider.GetYears()
                    .Select(y => int.TryParse(y, out int yr) ? yr : (int?)null)
                    .Where(yr => yr.HasValue)
                    .Select(yr => yr!.Value)];


        var oldRangeYears = yearRangeProvider.GetOldRangeYears();
        int youngest = oldRangeYears[0];
        int oldest = oldRangeYears[^1];
        


        List<(int Start, int End)> batches;

        if (overrideYears.Count > 0)
        {
            bool anyInOldRange = overrideYears.Any(y => oldRangeYears.Contains(y));

            if (anyInOldRange)
            {
                batches = [(oldest, youngest)];
            }
            else
            {
                batches = [.. overrideYears.Select(y => (y, y))];
            }
        }
        else
        {
            List<int> defaultYears = [.. yearRangeProvider.GetYears().Select(y => int.Parse(y))];
            batches = [.. defaultYears.Select(y => (y, y))];
        }

        var tasks = batches.Select(range => FetchRangeAndParse(range.Start, range.End, date.Month, date.Day, ct));

        var results = await Task.WhenAll(tasks);
        List<Person> allPeople = [.. results.SelectMany(p => p)];

        if (date.Month == 2 && date.Day == 29)
        {
            HashSet<int> leapYears =
            [
                .. yearRangeProvider.GetLeapYears()
                .Select(y => int.TryParse(y, out int yr) ? yr : 0)
                .Where(yr => yr > 0)
            ];

            allPeople = [.. allPeople.Where(p => leapYears.Contains(p.BirthYear))];
        }

        if (overrideYears.Count > 0)
        {
            allPeople = [.. allPeople.Where(p => overrideYears.Contains(p.BirthYear))];
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