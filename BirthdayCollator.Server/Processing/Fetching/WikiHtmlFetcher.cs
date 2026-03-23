namespace BirthdayCollator.Server.Processing.Fetching;

public sealed class WikiHtmlFetcher(IHttpClientFactory factory)
{
    private readonly HttpClient _http = factory.CreateClient(HttpClients.Wikipedia);

    public async Task<string> FetchHtmlAsync(string pageName, CancellationToken ct)
    {
        try
        {
            string url = $"{Urls.API}/{pageName}";

            using HttpResponseMessage response = await _http.GetAsync(url, ct);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[HTTP {response.StatusCode}] Page not found or error: {pageName}");
                return string.Empty; 
            }

            return await response.Content.ReadAsStringAsync(ct);
        }
        catch (OperationCanceledException)
        {
            return string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UNEXPECTED ERROR] {pageName}: {ex.Message}");
            return string.Empty;
        }
    }
}