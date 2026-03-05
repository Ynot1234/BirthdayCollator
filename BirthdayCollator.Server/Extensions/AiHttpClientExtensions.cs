using BirthdayCollator.Server.AI.Semantic.Embeddings;

namespace BirthdayCollator.Server.Extensions;


public static class AiHttpClientExtensions
{
    public static IServiceCollection AddAiHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<IEmbeddingService, OpenAIEmbeddingService>();
        return services;
    }
}
