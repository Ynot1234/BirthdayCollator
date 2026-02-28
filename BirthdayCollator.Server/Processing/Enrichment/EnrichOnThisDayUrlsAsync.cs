using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Models;
using System.Text.Json;

namespace BirthdayCollator.Server.Processing.Enrichment;

public sealed class PersonWikiEnricher(IHttpClientFactory httpFactory)
{
    private readonly HttpClient _wikiClient = httpFactory.CreateClient("WikiClient");

    public async Task<List<Person>> EnrichOnThisDayUrlsAsync(List<Person> people)
    {
        foreach (Person p in people)
        {
            if (!string.Equals(p.SourceSlug, "OnThisDay", StringComparison.OrdinalIgnoreCase))
                continue;

            var (wikiTitle, wikiUrl) = await LookupWikiTitleAndUrlAsync(p.Name, p.Description);

            if (wikiTitle is null || wikiUrl is null)
                continue;

            if (!IsLikelySamePerson(p.Name, wikiTitle))
                continue;

            p.Url = wikiUrl;
        }

        return people;
    }

    private async Task<(string? Title, string? Url)> LookupWikiTitleAndUrlAsync(string name, string description)
    {
        string desc = GetFirstTwoWords(description);
        string query = $"{name} {desc}".Trim();

        string api = $"{Urls.Domain}/{Urls.APISearchStub}{Uri.EscapeDataString(query)}&format=json";

        string json = await _wikiClient.GetStringAsync(api);

        using var result = JsonDocument.Parse(json);

        JsonElement first = result.RootElement
            .GetProperty("query")
            .GetProperty("search")
            .EnumerateArray()
            .FirstOrDefault();

        if (first.ValueKind == JsonValueKind.Undefined)
            return (null, null);

        string title = first.GetProperty("title").GetString()!;
        string url = $"{Urls.ArticleBase}/{title.Replace(' ', '_')}";

        return (title, url);
    }

    private static bool IsLikelySamePerson(string sourceName, string wikiTitle)
    {
        var s = Normalize(sourceName).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var w = Normalize(wikiTitle).Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (s.Length < 2 || w.Length < 2)
            return false;

        string wikiLast = w.Reverse()
                           .FirstOrDefault(token => token.All(char.IsLetter))
                           ?? w[^1];

        string sourceLast = s[^1];

        if (!string.Equals(sourceLast, wikiLast, StringComparison.OrdinalIgnoreCase))
            return false;

        return FirstNameSimilar(s[0], w[0]);
    }

    private static bool FirstNameSimilar(string a, string b)
    {
        a = a.ToLowerInvariant();
        b = b.ToLowerInvariant();

        if (a == b)
            return true;

        if (b.StartsWith(a) || a.StartsWith(b))
            return true;

        return false;
    }

    private static string Normalize(string name)
    {
        return name
            .ToLowerInvariant()
            .Replace(".", "")
            .Replace(",", "")
            .Trim();
    }

    private static string GetFirstTwoWords(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return string.Empty;

        string[] words = description.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (words.Length >= 2)
            return $"{words[0]} {words[1]}";

        return words[0];
    }
}