using System.Collections.Concurrent;

namespace BirthdayCollator.Server.Processing.Pipelines;

public sealed class GenariansEngine(GenarianFetcher loader, IYearRangeProvider yearRangeProvider)
{
    public async Task<List<Person>> ScrapeAllAsync(string monthName, int day, CancellationToken ct)
    {
        var years = yearRangeProvider.GetGenarianYears();

        if (day == 29)
        {
            years = [.. years.Where(y => int.TryParse(y, out int yr) && DateTime.IsLeapYear(yr))];
        }

        ConcurrentBag<List<Person>> results = [];

        await Parallel.ForEachAsync(
            years,
            new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = ct },
            async (year, token) =>
            {
                var pageResults = await loader.LoadPageAsync(year, monthName, day, token);
                results.Add(pageResults);
            });

        return [.. results.SelectMany(r => r)];
    }
}