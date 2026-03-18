using BirthdayCollator.Server.Processing.Cleaning;
using BirthdayCollator.Server.Processing.Deduplication;
using BirthdayCollator.Server.Processing.Sorting;
using Microsoft.Extensions.Caching.Hybrid;

namespace BirthdayCollator.Server.Extensions;

public static class BirthdayServicesExtensions
{
    public static IServiceCollection AddBirthdayCollator(this IServiceCollection services)
    {
        services.AddBirthdayCors()
                .AddWikiHttpClients()
                .AddBirthdayCore()
                .AddBirthdayHelpers()
                .AddBirthdayFactories()
                .AddBirthdayParsers()
                .AddBirthdayPipelines()
                .AddBirthdaySources()
                .AddBirthdayProcessing()
                .AddBirthdayAi();

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHealthChecks();
        services.AddMemoryCache();
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromDays(7),
                LocalCacheExpiration = TimeSpan.FromDays(7)
            };
        });
        return services;
    }

    public static WebApplication UseBirthdayPipeline(this WebApplication app)
    {
        app.MapHealthChecks("/health");

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseCors("AllowFrontend");

        app.MapControllers();
        app.MapFallbackToFile("index.html");

        return app;
    }

    public static IServiceCollection AddBirthdayProcessing(this IServiceCollection services)
    {
        services.AddScoped<PersonDeduper>();
        services.AddScoped<PersonCleaner>();
        services.AddScoped<PersonFilter>();
        services.AddScoped<PersonSorter>();
        services.AddScoped<NearDuplicateRemover>();
        services.AddScoped<PersonAIEnricher>();
        services.AddScoped<PersonWikiEnricher>();
        services.AddScoped<PersonFactory>();
        services.AddScoped<OnThisDayParser>();
        services.AddScoped<GenariansPageParser>();
        services.AddScoped<GenarianFetcher>();
        services.AddScoped<WikiHtmlFetcher>();
        return services;
    }
}