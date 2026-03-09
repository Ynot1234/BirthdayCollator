using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Parsers;
using HtmlAgilityPack;

namespace BirthdayCollator.Server.Resources;

public sealed class GenarianPageLoader(
    IHttpClientFactory httpFactory,
    GenariansPageParser parser,
    PersonFactory personFactory)
{
    private readonly HttpClient _http = httpFactory.CreateClient("WikiClient");

    public async Task<List<Person>> LoadPageAsync(string year, string month, int day, CancellationToken ct)
    {
        string url = $"{Urls.GenarianBase}/{year}.html";
        string html = await _http.GetStringAsync(url, ct);

        HtmlDocument doc = new();
        doc.LoadHtml(html);

        var rows = doc.DocumentNode.SelectNodes("//tr[th]");
        if (rows is null) return [];

        return [.. rows
            .Select(row => parser.TryParseRow(row, month, day, url, out var p)
                ? personFactory.Finalize(p!)
                : null)
            .OfType<Person>()];
    }
}