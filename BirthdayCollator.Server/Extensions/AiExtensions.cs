using BirthdayCollator.AI.Semantic;
using BirthdayCollator.Server.AI.Services;
using BirthdayCollator.Server.Processing.Enrichment;

namespace BirthdayCollator.Server.Extensions;

public static class AiExtensions
{
    public static IServiceCollection AddBirthdayAi(this IServiceCollection services, IConfiguration config)
    {
        services.AddSemanticKernel(config);
        services.AddScoped<IAIService, AIService>();

        services.AddMemoryCache();
        services.AddScoped<IPersonEnrichmentService, PersonEnrichmentService>();


        return services;
    }
}
