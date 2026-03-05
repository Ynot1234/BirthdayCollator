using BirthdayCollator.Server.AI.Semantic.VectorDb;

namespace BirthdayCollator.Server.Extensions;

public static class AiVectorStoreExtensions
{
    public static IServiceCollection AddVectorStore(
        this IServiceCollection services,
        IConfiguration config)
    {
        var qdrantUrl = config["Qdrant:Url"]!;
        var qdrantKey = config["Qdrant:ApiKey"]!;

        services.AddHttpClient<QdrantClient>(client =>
        {
            client.BaseAddress = new Uri(qdrantUrl);
            client.DefaultRequestHeaders.Add("api-key", qdrantKey);
        });

        services.AddSingleton<IVectorStore, QdrantVectorStore>();
        services.AddHostedService<QdrantInitializer>();

        return services;
    }
}
