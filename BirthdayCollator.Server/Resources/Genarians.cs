using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Helpers;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Parsers;
using HtmlAgilityPack;
using System.Globalization;

namespace BirthdayCollator.Server.Resources;

public sealed class Genarians(
    PersonFactory personFactory,
    GenariansPageParser parser,
    IYearRangeProvider yearRangeProvider)
{
    public async Task<List<Person>> ScrapeGenariansPageAsync(
        string url,
        string  targetMonthName,
        int targetDay,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        HtmlWeb web = new();
        HtmlDocument doc = await web.LoadFromWebAsync(url, token);

        HtmlNodeCollection rows = doc.DocumentNode.SelectNodes("//tr[th]");
        List<Person> results = [];

        if (rows == null)
            return results;

        foreach (HtmlNode row in rows)
        {
            token.ThrowIfCancellationRequested();

            if (parser.TryParseRow(row, targetMonthName, targetDay, url, out var parsed))
            {
                Person person = personFactory.CreateFromParsedGenarian(parsed);
                results.Add(person);
            }
        }

        return results;
    }

    private static string BuildGenarianUrl(int year)
    {
        return $"{Urls.GenarianBase}/{year}.html";
    }

    public async Task<List<Person>> ScrapeAllGenariansAsync(
        string targetMonthName,
        int targetDay,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        int month = DateTime.ParseExact(
            targetMonthName,
            "MMMM",
            CultureInfo.InvariantCulture).Month;

        List<Person> people = [];

            IReadOnlyList<string> years = yearRangeProvider.GetYears();
            var normal = await ScrapeYearSetAsync(years, targetMonthName, targetDay, token);
            people.AddRange(normal);
    
        if (LeapYear.IsNonLeapFeb28(month, targetDay))
        {
            IReadOnlyList<string> leapYears = yearRangeProvider.GetLeapYears();
            var feb29 = await ScrapeYearSetAsync(leapYears, targetMonthName, targetDay + 1, token);
            people.AddRange(feb29);
        }

        return people;
    }

    private async Task<List<Person>> ScrapeYearSetAsync(
    IReadOnlyList<string> years,
    string targetMonthName,
    int targetDay,
    CancellationToken token)
    {
        List<Task<List<Person>>> tasks = [];

        foreach (string yearStr in years)
        {
            token.ThrowIfCancellationRequested();

            if (!int.TryParse(yearStr, out int year))
                continue;

            string url = BuildGenarianUrl(year);
            tasks.Add(ScrapeGenariansPageAsync(url, targetMonthName, targetDay, token));
        }

        List<Person>[] results = await Task.WhenAll(tasks);

        HashSet<int> allowedYears = [.. years
        .Select(y => int.TryParse(y, out int yr) ? yr : -1)
        .Where(yr => yr > 0)];

        return [.. results
        .SelectMany(r => r)
        .Where(p => allowedYears.Contains(p.BirthYear))];
    }


}