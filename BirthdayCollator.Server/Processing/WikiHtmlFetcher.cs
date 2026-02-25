using BirthdayCollator.Constants;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BirthdayCollator.Processing
{
    public class WikiHtmlFetcher(HttpClient httpClient)
    {
        private readonly HttpClient _client = httpClient;

        public async Task<string> FetchHtmlAsync(string pageName, CancellationToken token)
        {
            string url = $"{Urls.API}/{pageName}";

            HttpResponseMessage response = await _client.GetAsync(url, token);

            response.EnsureSuccessStatusCode();

            string html = await response.Content.ReadAsStringAsync(token);

            return html;
        }
    }
}
