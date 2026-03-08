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
        var collectionName = config["Qdrant:CollectionName"] ?? "birthdaycollator"; 

        services.AddHttpClient<QdrantClient>(client =>
        {
            client.BaseAddress = new Uri(qdrantUrl);
            client.DefaultRequestHeaders.Add("api-key", qdrantKey);
        })
        .AddTypedClient((httpClient, sp) =>
        {
            return new QdrantClient(httpClient, collectionName);
        });

        services.AddSingleton<IVectorStore, QdrantVectorStore>();
        services.AddHostedService<QdrantInitializer>();

        return services;
    }

}
