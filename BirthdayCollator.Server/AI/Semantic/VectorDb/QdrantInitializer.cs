namespace BirthdayCollator.Server.AI.Semantic.VectorDb;

public class QdrantInitializer(QdrantClient client) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // 1. Pass the record, not the int
        await client.EnsureCollectionAsync(new VectorConfig(1536));

        // 2. Pass the cancellationToken to the first parameter
        await client.WaitUntilCollectionExistsAsync(cancellationToken);

        // 3. This will now be recognized
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