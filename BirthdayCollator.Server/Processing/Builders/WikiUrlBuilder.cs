using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BirthdayCollator.Server.Processing.Builders
{
    public class WikiUrlBuilder()
    {
        private static readonly HttpClient _redirectClient = new(new HttpClientHandler
        {
            AllowAutoRedirect = true
        });

        private static readonly Dictionary<string, string> _redirectCache =
            new(StringComparer.OrdinalIgnoreCase);

        // ------------------------------------------------------------
        // NORMALIZE HREF
        // ------------------------------------------------------------
        public static string NormalizeWikiHref(string href)
        {
            if (string.IsNullOrWhiteSpace(href))
                return href;

            // Full URL? Leave it alone.
            if (href.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return href;

            // "./Name" → "/wiki/Name"
            if (href.StartsWith("./"))
                return "/wiki/" + href.Substring(2);

            // Already a wiki link? Leave it alone.
            if (href.StartsWith("/wiki/"))
                return href;

            // Fallback: treat as article name
            return "/wiki/" + href.TrimStart('/');
        }

     

        // ------------------------------------------------------------
        // redirect resolution - not used yet
        // ------------------------------------------------------------
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
}
