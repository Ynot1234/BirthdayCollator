using BirthdayCollator.Server.Constants;

namespace BirthdayCollator.Server.Processing.Fetching;

public sealed class ImdbFetcher(HttpClient http)
{
    public async Task<string> FetchAsync(int year, int month, int day, CancellationToken cancellationToken)
    {
        string targetDate = $"{year:D4}-{month:D2}-{day:D2}";

        string url = $"{Urls.ImdbSearchStub}={targetDate},{targetDate}&has=!death-date&sort=starmeter,asc";

        using HttpRequestMessage request = new(HttpMethod.Get, url);
        
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

        using HttpResponseMessage response =  await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}