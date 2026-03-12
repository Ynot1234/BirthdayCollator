using BirthdayCollator.Server.Processing.Fetching;
using static BirthdayCollator.Server.Constants.AppStrings;

namespace BirthdayCollator.Server.Extensions;   
public static class HttpClientExtensions
{
    private static void ConfigureWikiClient(HttpClient client)
    {
        client.Timeout = Timeout.InfiniteTimeSpan;
        client.DefaultRequestHeaders.UserAgent.Clear();
        client.DefaultRequestHeaders.Add("User-Agent", "BirthdayCollator/1.0 (anthony@example.com)");
    }

    public static IServiceCollection AddWikiHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<OnThisDayHtmlFetcher>(ConfigureWikiClient);
        services.AddHttpClient(HttpClients.Wikipedia, ConfigureWikiClient)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = true });

        return services;
    }
}