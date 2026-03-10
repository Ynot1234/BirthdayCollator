using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Html;
using BirthdayCollator.Server.Processing.Links;
using BirthdayCollator.Server.Helpers; 

namespace BirthdayCollator.Server.Processing.Enrichment;

public sealed class PersonWikiEnricher(IHttpClientFactory httpFactory)
{
    private readonly HttpClient _http = httpFactory.CreateClient("WikiClient");

    public async Task<List<Person>> EnrichOnThisDayUrlsAsync(List<Person> people, CancellationToken ct)
    {
        // 1. Isolate the "OnThisDay" list
        var targets = people
            .Where(p => string.Equals(p.SourceSlug, "OnThisDay", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // 2. Resolve Wiki links in parallel
        await Parallel.ForEachAsync(
            targets,
            new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = ct },
            async (p, token) =>
            {
                var (title, url) = await ResolveWikiMatchAsync(p.Name, p.Description, token);

                if (title != null && WikiValidator.HrefMatchesName(title, p.Name))
                {
                    p.Url = url!;
                }
            });

        // 3. CLEANUP: This is where we call the Helper class
        foreach (var p in people)
        {
            p.Url = UrlNormalization.Fix(p.Url, p.Name);
        }

        return people;
    }

    private async Task<(string? Title, string? Url)> ResolveWikiMatchAsync(string name, string description, CancellationToken ct)
    {
        string query = $"{name} {WikiTextUtility.GetFirstTwoWords(description)}".Trim();
        string api = $"{Urls.Domain}{Urls.APISearchStub}{Uri.EscapeDataString(query)}&format=json";

        try
        {
            var response = await _http.GetFromJsonAsync<WikiSearchResponse>(api, ct);
            var first = response?.Query?.Search?.FirstOrDefault();

            if (first is null) return (null, null);

            return (first.Title, $"{Urls.ArticleBase}/{first.Title.Replace(' ', '_')}");
        }
        catch { return (null, null); }
    }

    private record WikiSearchResponse(WikiQuery Query);
    private record WikiQuery(List<WikiSearchResult> Search);
    private record WikiSearchResult(string Title);
}