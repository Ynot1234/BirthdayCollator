using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Parsers;
using HtmlAgilityPack;

namespace BirthdayCollator.Server.Resources;

public sealed class Genarians(
    PersonFactory personFactory,
    GenariansPageParser parser,
    IYearRangeProvider yearRangeProvider)
{
    public async Task<List<Person>> ScrapeGenariansPageAsync(
        string url,
        string? targetMonthName,
        int? targetDay,
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

    public async Task<List<Person>> ScrapeAllGenariansAsync(string? targetMonthName, int? targetDay, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        IReadOnlyList<string> years = yearRangeProvider.GetYears();

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