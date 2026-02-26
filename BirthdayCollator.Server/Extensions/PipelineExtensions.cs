using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Dates;
using BirthdayCollator.Server.Processing.Entries;
using BirthdayCollator.Server.Processing.Html;
using BirthdayCollator.Server.Processing.Links;
using BirthdayCollator.Server.Processing.Names;
using BirthdayCollator.Server.Processing.Parsers;
using BirthdayCollator.Server.Processing.Pipelines;
using BirthdayCollator.Server.Processing.Sources;
using BirthdayCollator.Server.Resources;
using BirthdayCollator.Server.Services;

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
        services.AddScoped<BirthdayFetcher>();
        services.AddScoped<Genarians>();
        services.AddScoped<GenariansPageParser>();
        services.AddScoped<BirthSourceEngine>();
        services.AddScoped<IPersonPipeline, PersonPipeline>();
        services.AddScoped<IFetchPipeline, FetchPipeline>();

        // Pipeline components
        services.AddScoped<BirthSourceEngine>();
        services.AddScoped<IFetchPipeline, FetchPipeline>();
        services.AddScoped<IPersonPipeline, PersonPipeline>();

        services.AddSingleton<IYearRangeProvider, YearRangeProvider>();
        services.AddSingleton<IEntrySplitter, EntrySplitter>();
        services.AddSingleton<ILinkResolver, LinkResolver>();
        services.AddSingleton<IPersonNameResolver, PersonNameResolver>();
        services.AddSingleton<IHtmlBirthSectionExtractor, HtmlBirthSectionExtractor>();
        services.AddSingleton<IBirthDateParser, BirthDateParser>();
        
        services.AddSingleton<Func<string, string>>(sp => WikiUrlBuilder.NormalizeWikiHref);

        return services;
    }
}