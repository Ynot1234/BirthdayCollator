using BirthdayCollator.Server.Constants;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BirthdayCollator.Server.AI.Semantic.Embeddings;

public class OpenAIEmbeddingService : IEmbeddingService
{
    private readonly HttpClient http;
    private readonly string apiKey;
    private readonly string model;

    public OpenAIEmbeddingService(IConfiguration config, HttpClient httpClient)
    {
        this.http = httpClient;
        this.apiKey = config["OpenAI:ApiKey"] ?? throw new InvalidOperationException("Missing OpenAI:ApiKey");
        this.model = config["OpenAI:EmbeddingModel"] ?? "text-embedding-3-small";
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<float[]> EmbedAsync(string text)
    {
        var requestBody = new
        {
            model,
            input = text
        };

        var response = await http.PostAsJsonAsync(Urls.OpenAIEmbeddings, requestBody);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<EmbeddingResponse>(json);

        return result?.Data?[0]?.Embedding 
            ?? throw new Exception("Failed to parse embedding response.");
    }

    private class EmbeddingResponse
    {
        [JsonPropertyName("data")]
        public List<EmbeddingData>? Data { get; set; }
    }

    private class EmbeddingData
    {
        [JsonPropertyName("embedding")]
        public float[]? Embedding { get; set; }
    }
}
