using System.Web;

namespace BirthdayCollator.Server.Processing.Fetching;

public sealed class ImdbFetcher(HttpClient http)
{
    public async Task<string> FetchRangeAsync(string yearRange, int month, int day, CancellationToken ct)
    {
        string cacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HtmlCache");
        if (!Directory.Exists(cacheDir)) Directory.CreateDirectory(cacheDir);

        string safeRange = yearRange.Replace(",", "-");
        string fileName = $"{month:D2}-{day:D2}_{safeRange}.html";
        string filePath = Path.Combine(cacheDir, fileName);

        if (File.Exists(filePath))
        {
            Console.WriteLine($"[Cache Hit] Loading {fileName} from disk...");
            return await File.ReadAllTextAsync(filePath, ct);
        }

        string _apiKey = "FJOFF66F2WA3KD4E10UY5FZZUXNQWT6983G24EZE68LV8TTLUOFL6IUTT80ZFMQAVDRPMGAN89ZFFSD8";

        try
        {
            string targetUrl = $"{Urls.ImdbBase}/search/name/?" +
                               $"birth_monthday={month:D2}-{day:D2}&" +
                               $"birth_date={yearRange}&" +
                               $"has=!death-date&" +
                               $"sort=starmeter,asc";

            var queryParams = HttpUtility.ParseQueryString(string.Empty);
            queryParams["api_key"] = _apiKey;
            queryParams["url"] = targetUrl;
            queryParams["render_js"] = "true";
            queryParams["premium_proxy"] = "true";
            queryParams["stealth_proxy"] = "true";
            queryParams["block_resources"] = "false";
            queryParams["wait"] = "0";
            queryParams["wait_for"] = ".ipc-metadata-list-item";
            queryParams["device"] = "desktop";

            string scrapingBeeUrl = $"{Urls.ScrapingBeeBase}?{queryParams}";

            using HttpRequestMessage request = new(HttpMethod.Get, scrapingBeeUrl);
            using HttpResponseMessage response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

            if (!response.IsSuccessStatusCode)
            {
                return "";
            }

            string html = await response.Content.ReadAsStringAsync(ct);
            await File.WriteAllTextAsync(filePath, html, ct);
            Console.WriteLine($"[Cache Miss] Saved {fileName} to disk.");
            return html;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to fetch range {yearRange}: {ex.Message}");
            throw;
        }
    }
}