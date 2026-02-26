using BirthdayCollator.Server.Constants;
using System.Globalization;

namespace BirthdayCollator.Server.Processing.Fetching;

public sealed class OnThisDayHtmlFetcher(HttpClient http)
{
    public async Task<string> FetchAsync(int month, int day, CancellationToken cancellationToken)
    {
        string monthName = CultureInfo.InvariantCulture.DateTimeFormat
            .GetMonthName(month)
            .ToLowerInvariant();

        string url = $"{Urls.OnThisDayBase}/{monthName}/{day}";

        using HttpRequestMessage request = new(HttpMethod.Get, url);

        using HttpResponseMessage response =
            await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

}
