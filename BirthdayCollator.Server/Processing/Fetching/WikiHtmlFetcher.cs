using BirthdayCollator.Server.Constants;
using static BirthdayCollator.Server.Constants.AppStrings;

namespace BirthdayCollator.Server.Processing.Fetching;

public sealed class WikiHtmlFetcher(IHttpClientFactory factory)
{
    private readonly HttpClient _http = factory.CreateClient(HttpClients.Wikipedia);

    public async Task<string> FetchHtmlAsync(string pageName, CancellationToken ct)
    {
        return await _http.GetStringAsync($"{Urls.API}/{pageName}", ct);
    }
}