namespace BirthdayCollator.Server.AI.Semantic.VectorDb;

public class QdrantVectorStore(QdrantClient client) : IVectorStore
{

    public Task UpsertAsync(string personId, string chunkId, string text, float[] embedding)
    {
        return client.UpsertAsync(
            chunkId,
            embedding,
            new { personId, text }
        );
    }

    public async Task<List<(string Text, float Score)>> SearchAsync(
      string personId,
      float[] queryEmbedding,
      int topK)
    {
        // 1. Await the client's result (List<ScoredPoint>)
        var points = await client.SearchAsync(
            personId,
            queryEmbedding,
            topK);

        // 2. Map the ScoredPoint objects to the Tuple format
        return [.. points.Select(p =>
        {
            string textValue = "N/A";

            // Extract "text" from the payload dictionary safely
            if (p.Payload != null && p.Payload.TryGetValue("text", out var val))
            {
                textValue = val?.ToString() ?? "N/A";
            }

            return (Text: textValue, p.Score);
        })];
    }

}
