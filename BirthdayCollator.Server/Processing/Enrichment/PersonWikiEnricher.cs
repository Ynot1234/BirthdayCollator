namespace BirthdayCollator.Server.Processing.Enrichment;

public sealed class PersonWikiEnricher(IHttpClientFactory httpFactory)
{
    private readonly HttpClient _http = httpFactory.CreateClient(HttpClients.Wikipedia);

    public async Task<List<Person>> EnrichOnThisDayUrlsAsync(List<Person> people, CancellationToken ct)
    {
        var targets = people.Where(p => string.Equals(p.SourceSlug, Slugs.OnThisDay, StringComparison.OrdinalIgnoreCase)).ToList();

        if (targets.Count is 0) return people;

        ParallelOptions options = new() { MaxDegreeOfParallelism = 4, CancellationToken = ct };

        await Parallel.ForEachAsync(targets, options, async (p, token) =>
        {
            var (title, url) = await ResolveWikiMatchAsync(p.Name, p.Description, token);

            if (title is not null && WikiValidator.HrefMatchesName(title, p.Name))
            {
                p.Url = url!;
            }
        });

        foreach (var p in people)
        {
            p.Url = UrlNormalization.Fix(p.Url, p.Description, p.Name);
        }

        return people;
    }

    private async Task<(string? Title, string? Url)> ResolveWikiMatchAsync(string name, string description, CancellationToken ct)
    {
        string descSnippet = string.Empty;

        if (!string.IsNullOrWhiteSpace(description))
        {
            ReadOnlySpan<char> span = description.AsSpan().Trim();
            int firstSpace = span.IndexOf(' ');
            if (firstSpace != -1)
            {
                int secondSpace = span[(firstSpace + 1)..].IndexOf(' ');
                int length = secondSpace == -1 ? span.Length : firstSpace + 1 + secondSpace;
                descSnippet = new string(span[..length]);
            }
            else
            {
                descSnippet = new string(span);
            }
        }

        string query = $"{name} {descSnippet}".Trim();
        string api = $"{Urls.Domain}{Urls.APISearchStub}{Uri.EscapeDataString(query)}&format=json";

        var response = await _http.GetFromJsonAsync<WikiSearchResponse>(api, ct);
        var results = response?.Query?.Search;

        if (results is null or { Count: 0 }) return (null, null);

        var bestMatch = results.FirstOrDefault(s =>
            string.Equals(s.Title, name, StringComparison.OrdinalIgnoreCase) ||
            s.Title.StartsWith($"{name} (", StringComparison.OrdinalIgnoreCase))
            ?? results[0];

        return (bestMatch.Title, $"{Urls.ArticleBase}/{bestMatch.Title.Replace(' ', '_')}");
    }

    private record WikiSearchResponse(WikiQuery? Query);
    private record WikiQuery(List<WikiSearchItem> Search);
    private record WikiSearchItem(string Title, int Wordcount, int Size, string Snippet);
}