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

    public Task<List<(string Text, float Score)>> SearchAsync(
        string personId,
        float[] queryEmbedding,
        int topK)
    {
        return client.SearchAsync(
                                  personId,
                                  queryEmbedding,
                                  topK);

    }
}
