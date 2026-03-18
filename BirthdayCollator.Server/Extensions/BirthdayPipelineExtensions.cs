using BirthdayCollator.Server.Services;

namespace BirthdayCollator.Server.Extensions;

public static class BirthdayPipelineExtensions
{
    public static IServiceCollection AddBirthdayPipelines(this IServiceCollection services)
    {
        services.AddScoped<BirthSourceEngine>();
        services.AddScoped<IFetchPipeline, FetchPipeline>();
        services.AddScoped<IPersonPipeline, PersonPipeline>();
        services.AddScoped<BirthdayFetcher>();
        services.AddScoped<GenariansEngine>();
        return services;
    }
}