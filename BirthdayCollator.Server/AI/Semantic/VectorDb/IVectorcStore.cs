namespace BirthdayCollator.Server.AI.Semantic.VectorDb;

public interface IVectorStore
{
    Task UpsertAsync(string personId, string chunkId, string text, float[] embedding);
    Task<List<(string Text, float Score)>> SearchAsync(string personId, float[] queryEmbedding, int topK);
}
