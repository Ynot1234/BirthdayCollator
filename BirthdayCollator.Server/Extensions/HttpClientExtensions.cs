namespace BirthdayCollator.Server.Extensions;   
public static class HttpClientExtensions
{
    private static void ConfigureClient(HttpClient client)
    {
        client.Timeout = Timeout.InfiniteTimeSpan;
        client.DefaultRequestHeaders.UserAgent.Clear();
        client.DefaultRequestHeaders.Add("User-Agent", "BirthdayCollator/1.0 (anthony@example.com)");
    }

    public static IServiceCollection AddWikiHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<OnThisDayHtmlFetcher>(ConfigureClient);
        services.AddHttpClient(HttpClients.Wikipedia, ConfigureClient)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = true });

        return services;
    }
}