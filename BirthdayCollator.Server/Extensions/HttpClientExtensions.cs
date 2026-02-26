using BirthdayCollator.Server.Processing.Fetching;

namespace BirthdayCollator.Server.Extensions;

public static class HttpClientExtensions
{
    private static void ConfigureWikiClient(HttpClient client)
    {
        client.Timeout = Timeout.InfiniteTimeSpan;
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "BirthdayApp/1.0 (https://github.com/anthony/birthdayapp; anthony@example.com)");
    }

    public static IServiceCollection AddWikiHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<OnThisDayHtmlFetcher>(ConfigureWikiClient);

        services.AddHttpClient("WikiClient", ConfigureWikiClient)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = true
            });

        return services;
    }
}
