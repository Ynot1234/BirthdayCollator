namespace BirthdayCollator.Server.AI.Semantic.VectorDb;

public class QdrantInitializer(QdrantClient client) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await client.EnsureCollectionAsync(new VectorConfig(1536));
        await client.WaitUntilCollectionExistsAsync(ct: cancellationToken);
        await client.EnsurePersonIdIndexAsync();
    }

     public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}


public class QdrantResponse<T>
{
    public T Result { get; set; } = default!;
    public string Status { get; set; } = default!;
    public double Time { get; set; }
}