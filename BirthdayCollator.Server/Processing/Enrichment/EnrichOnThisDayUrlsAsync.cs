using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Html;
using BirthdayCollator.Server.Processing.Links;

namespace BirthdayCollator.Server.Processing.Enrichment;
public sealed class PersonWikiEnricher(IHttpClientFactory httpFactory, ILinkResolver linkResolver)
{
    private readonly HttpClient _http = httpFactory.CreateClient("WikiClient");

    public async Task<List<Person>> EnrichOnThisDayUrlsAsync(List<Person> people, CancellationToken ct)
    {
        var targets = people
            .Where(p => string.Equals(p.SourceSlug, "OnThisDay", StringComparison.OrdinalIgnoreCase))
            .ToList();

        await Parallel.ForEachAsync(
            targets,
            new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = ct },
            async (p, token) =>
            {
                var (title, url) = await LookupWikiTitleAndUrlAsync(p.Name, p.Description, token);

                if (title != null && linkResolver.HrefMatchesName(title, p.Name))
                    p.Url = url!;
            });

        foreach (var p in people)
        {
            if (string.IsNullOrWhiteSpace(p.Url) ||
                p.Url.Equals("null", StringComparison.OrdinalIgnoreCase) ||
                p.Url.Equals("undefined", StringComparison.OrdinalIgnoreCase))
            {
                p.Url = $"{Urls.DDGSearchBase}/?q={Uri.EscapeDataString(p.Name)}";
            }
            else if (!p.Url.StartsWith("http://") && !p.Url.StartsWith("https://"))
            {
                p.Url = "https://" + p.Url;
            }
        }

        return people;
    }

    private async Task<(string? Title, string? Url)> LookupWikiTitleAndUrlAsync(string name, string description, CancellationToken ct)
    {
            string query = $"{name} {WikiTextUtility.GetFirstTwoWords(description)}".Trim();
            string api = $"{Urls.Domain}{Urls.APISearchStub}{Uri.EscapeDataString(query)}&format=json";

            var response = await _http.GetFromJsonAsync<WikiSearchResponse>(api, ct);
            var first = response?.Query?.Search?.FirstOrDefault();

        if (first is null)
            return (null, null);

        string title = first.Title;
        return (title, $"{Urls.ArticleBase}/{title.Replace(' ', '_')}");
    }

    private record WikiSearchResponse(WikiQuery Query);
    private record WikiQuery(List<WikiSearchResult> Search);
    private record WikiSearchResult(string Title);
}