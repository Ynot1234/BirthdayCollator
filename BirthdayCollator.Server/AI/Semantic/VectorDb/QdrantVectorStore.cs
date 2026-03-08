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
        var points = await client.SearchAsync(
            personId,
            queryEmbedding,
            topK);

        return [.. points.Select(p =>
        {
            string textValue = "N/A";

            if (p.Payload != null && p.Payload.TryGetValue("text", out var val))
            {
                textValue = val?.ToString() ?? "N/A";
            }

            return (Text: textValue, p.Score);
        })];
    }

}