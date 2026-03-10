using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Parsers;
using HtmlAgilityPack;

namespace BirthdayCollator.Server.Resources;

public sealed class GenarianPageLoader(IHttpClientFactory httpFactory, GenariansPageParser parser, PersonFactory personFactory)
{
    private readonly HttpClient _http = httpFactory.CreateClient("WikiClient");

    public async Task<List<Person>> LoadPageAsync(string year, string month, int day, CancellationToken ct)
    {
        string resource = year;
        string url = $"{Urls.GenarianBase}/{resource}.html";
        string html = await _http.GetStringAsync(url, ct);
        HtmlDocument doc = new();
        doc.LoadHtml(html);
        var rows = doc.DocumentNode.SelectNodes("//tr[count(th) >= 3 and th[3]//span[2]]");
        if (rows is null) return [];

        return
        [
            .. rows
                .Select(row =>
                    parser.TryParseRow(row, month, day, url,out var p)
                        ? personFactory.Finalize(p!)
                        : null)
                .OfType<Person>()
        ];
    }
}