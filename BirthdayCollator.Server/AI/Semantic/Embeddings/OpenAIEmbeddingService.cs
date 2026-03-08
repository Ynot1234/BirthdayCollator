using BirthdayCollator.Server.Constants;
using System.Text.Json.Serialization;

namespace BirthdayCollator.Server.AI.Semantic.Embeddings;

public sealed class OpenAIEmbeddingService(IConfiguration config, HttpClient http) : IEmbeddingService
{
    private readonly string _model = config["OpenAI:EmbeddingModel"] ?? "text-embedding-3-small";
    private readonly string _apiKey = config["OpenAI:ApiKey"] ?? throw new InvalidOperationException("Missing OpenAI API Key");

    public async Task<float[]> EmbedAsync(string text)
    {
        var request = new { model = _model, input = text };
        http.DefaultRequestHeaders.Authorization = new("Bearer", _apiKey);
        var response = await http.PostAsJsonAsync(Urls.OpenAIEmbeddings, request);

        var result = await response.Content.ReadFromJsonAsync<OpenAiResponse>();
        return result?.Data?.FirstOrDefault()?.Embedding
               ?? throw new Exception("OpenAI returned an empty embedding.");
    }

    private record OpenAiResponse(List<EmbeddingData> Data);
    private record EmbeddingData([property: JsonPropertyName("embedding")] float[] Embedding);
}
