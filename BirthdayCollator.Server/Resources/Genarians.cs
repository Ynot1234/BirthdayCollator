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
        var years = yearRangeProvider.GetGenarianYears();

        var people = await ScrapeYearSetAsync(years, monthName, day, ct);

        int month = DateTime.ParseExact(monthName, "MMMM", CultureInfo.InvariantCulture).Month;

        if (LeapYear.IsNonLeapFeb28(month, day))
        {
            var leapYears = years.Where(y => int.TryParse(y, out int yr) && DateTime.IsLeapYear(yr));
            var leapPeople = await ScrapeYearSetAsync(leapYears, monthName, day + 1, ct);
            people.AddRange(leapPeople);
        }

        return people;
    }

    private async Task<List<Person>> ScrapeYearSetAsync(IEnumerable<string> years, string month, int day, CancellationToken ct)
    {
        var results = new ConcurrentBag<List<Person>>();

        await Parallel.ForEachAsync(
            years,
            new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = ct },
            async (year, token) =>
            {
                var pageResults = await loader.LoadPageAsync(year, month, day, token);
                results.Add(pageResults);
            });

        return [.. results.SelectMany(r => r)];
    }
}