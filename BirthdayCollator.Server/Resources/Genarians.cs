using BirthdayCollator.Server.Helpers;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using System.Collections.Concurrent;
using System.Globalization;

namespace BirthdayCollator.Server.Resources;

public sealed class Genarians(
    GenarianPageLoader loader,
    IYearRangeProvider yearRangeProvider)
{
    public async Task<List<Person>> ScrapeAllAsync(string monthName, int day, CancellationToken ct)
    {
        var people = await ScrapeYearSetAsync(yearRangeProvider.GetYears(), monthName, day, ct);

        int month = DateTime.ParseExact(monthName, "MMMM", CultureInfo.InvariantCulture).Month;

        if (LeapYear.IsNonLeapFeb28(month, day))
        {
            var leapPeople = await ScrapeYearSetAsync(yearRangeProvider.GetLeapYears(), monthName, day + 1, ct);
            people.AddRange(leapPeople);
        }

        return people;
    }

    private async Task<List<Person>> ScrapeYearSetAsync(IEnumerable<string> years, string month, int day, CancellationToken ct)
    {
        var results = new ConcurrentBag<List<Person>>();

        await Parallel.ForEachAsync(years, new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = ct },
            async (year, token) =>
            {
                // Delegate the actual web/HTML work to the loader
                var pageResults = await loader.LoadPageAsync(year, month, day, token);
                results.Add(pageResults);
            });

        return [.. results.SelectMany(r => r)];
    }
}