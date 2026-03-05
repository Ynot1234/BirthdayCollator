namespace BirthdayCollator.Server.AI.Semantic.Embeddings;

public interface IEmbeddingService
{
    Task<float[]> EmbedAsync(string text);
}

