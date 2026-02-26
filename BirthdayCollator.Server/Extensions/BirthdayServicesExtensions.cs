using BirthdayCollator.Processing;
using BirthdayCollator.Server.Processing;

namespace BirthdayCollator.Server.Extensions;

public static class BirthdayServicesExtensions
{
    public static IServiceCollection AddBirthdayCore(this IServiceCollection services)
    {
        services.AddScoped<PersonDeduper>();
        services.AddScoped<PersonCleaner>();
        services.AddScoped<PersonFilter>();
        services.AddScoped<PersonSorter>();
        services.AddScoped<NearDuplicateRemover>();
        services.AddScoped<DeduplicateByURL>();
        services.AddScoped<PersonDedupe>();
        services.AddScoped<PersonAIEnricher>();
        services.AddScoped<PersonWikiEnricher>();

        services.AddScoped<PersonFactory>();
        services.AddScoped<OnThisDayParser>();

        services.AddScoped<WikiHtmlFetcher>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new WikiHtmlFetcher(factory.CreateClient("WikiClient"));
        });

        return services;
    }
}
