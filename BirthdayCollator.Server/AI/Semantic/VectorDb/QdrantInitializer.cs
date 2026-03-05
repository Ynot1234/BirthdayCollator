namespace BirthdayCollator.Server.AI.Semantic.VectorDb;

public class QdrantInitializer(QdrantClient client) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // 1. Start creation
        await client.EnsureCollectionAsync(1536);

        // 2. Wait for it to be globally visible and "Green"
        await client.WaitUntilCollectionExistsAsync();

        // 3. Create the index using the corrected PUT method
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