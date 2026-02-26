using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Cleaning;
using BirthdayCollator.Server.Processing.Deduplication;
using BirthdayCollator.Server.Processing.Enrichment;
using BirthdayCollator.Server.Processing.Fetching;
using BirthdayCollator.Server.Processing.Parsers;
using BirthdayCollator.Server.Processing.Pipelines;
using BirthdayCollator.Server.Processing.Sorting;

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
