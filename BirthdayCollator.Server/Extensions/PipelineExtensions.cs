using BirthdayCollator.Processing;
using BirthdayCollator.Resources;
using BirthdayCollator.Server.Processing;
using BirthdayCollator.Services;

namespace BirthdayCollator.Server.Extensions;

public static class PipelineExtensions
{
    public static IServiceCollection AddBirthdayPipelines(this IServiceCollection services)
    {
        services.AddScoped<IBirthSource, YearBirthSource>();
        services.AddScoped<IBirthSource, DateBirthSource>();
        services.AddScoped<IBirthSource, CategoryBirthSource>();
        services.AddScoped<IBirthSource, GenariansBirthSource>();
        services.AddScoped<IBirthSource, OnThisDaySource>();

        services.AddScoped<BirthSourceEngine>();
        services.AddScoped<IPersonPipeline, PersonPipeline>();
        services.AddScoped<IFetchPipeline, FetchPipeline>();

        services.AddSingleton<IYearRangeProvider, YearRangeProvider>();

        services.AddScoped<BirthdayFetcher>();
        services.AddScoped<Genarians>();
        services.AddScoped<GenariansPageParser>();

        services.AddSingleton<Func<string, string>>(sp =>
            WikiUrlBuilder.NormalizeWikiHref);

        return services;
    }
}
