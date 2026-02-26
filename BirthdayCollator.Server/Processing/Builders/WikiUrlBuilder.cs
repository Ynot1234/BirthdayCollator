namespace BirthdayCollator.Server.Processing.Builders;

public class WikiUrlBuilder()
{
    private static readonly HttpClient _redirectClient = new(new HttpClientHandler
    {
        AllowAutoRedirect = true
    });

    private static readonly Dictionary<string, string> _redirectCache = new(StringComparer.OrdinalIgnoreCase);

    public static string NormalizeWikiHref(string href)
    {
        if (string.IsNullOrWhiteSpace(href))
            return href;

        if (href.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return href;

        if (href.StartsWith("./"))
            return string.Concat("/wiki/", href.AsSpan(2));

        if (href.StartsWith("/wiki/"))
            return href;

        return "/wiki/" + href.TrimStart('/');
    }

 
    public async Task<string> ResolveRedirectAsync(string url)
    {
        if (_redirectCache.TryGetValue(url, out string? cached))
            return cached;

        HttpResponseMessage response = await _redirectClient.GetAsync(url);
        string finalUrl = response.RequestMessage!.RequestUri!.ToString();

        _redirectCache[url] = finalUrl;
        return finalUrl;
    }
}