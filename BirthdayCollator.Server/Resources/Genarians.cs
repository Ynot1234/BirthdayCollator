using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Helpers;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Parsers;
using HtmlAgilityPack;
using System.Collections.Concurrent;
using System.Globalization;

namespace BirthdayCollator.Server.Resources;

public sealed class Genarians(
    PersonFactory personFactory,
    GenariansPageParser parser,
    IYearRangeProvider yearRangeProvider,
    IHttpClientFactory httpFactory)
{
    private readonly HttpClient _http = httpFactory.CreateClient("WikiClient");

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

        await Parallel.ForEachAsync(
            years,
            new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = ct },
            async (year, token) =>
            {
                var pageResults = await ScrapePageAsync($"{Urls.GenarianBase}/{year}.html", month, day, token);
                results.Add(pageResults);
            });

        return [.. results.SelectMany(r => r)];
    }

    private async Task<List<Person>> ScrapePageAsync(string url, string month, int day, CancellationToken ct)
    {
            string html = await _http.GetStringAsync(url, ct);
            HtmlDocument doc = new();
            doc.LoadHtml(html);

            var rows = doc.DocumentNode.SelectNodes("//tr[th]");

            if (rows is null)
                return [];

            return rows
                .Select(row => parser.TryParseRow(row, month, day, url, out var p)
                    ? personFactory.Finalize(p!)
                    : null)
                .Where(p => p is not null)
                .ToList()!;
    }
}