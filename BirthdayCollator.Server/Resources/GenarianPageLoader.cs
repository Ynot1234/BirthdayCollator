using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Parsers;
using HtmlAgilityPack;
using static BirthdayCollator.Server.Constants.AppStrings;

namespace BirthdayCollator.Server.Resources;

public sealed class GenarianPageLoader(
    IHttpClientFactory httpFactory,
    GenariansPageParser parser,
    PersonFactory personFactory)
{
    private readonly HttpClient _http = httpFactory.CreateClient(HttpClients.Genarians);

    public async Task<List<Person>> LoadPageAsync(string year, string month, int day, CancellationToken ct)
    {
        string absoluteUrl = $"{Urls.GenarianBase}/{year}.html";

        string html = await _http.GetStringAsync(absoluteUrl, ct);

        HtmlDocument doc = new();
        doc.LoadHtml(html);

        var rows = doc.DocumentNode.SelectNodes("//tr[count(th) >= 3 and th[3]//span[2]]");

        if (rows is null) return [];

        return [.. rows
            .Select(row => parser.TryParseRow(row, month, day, absoluteUrl, out var p)
                ? personFactory.Finalize(p!)
                : null)
            .OfType<Person>()];
    }
}