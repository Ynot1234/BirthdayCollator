using System.Text.Json.Nodes;

namespace BirthdayCollator.Server.AI.Semantic.VectorDb;

public record VectorConfig(int Size, string Distance = "Cosine");
public record ScoredPoint(string Id, float Score, Dictionary<string, object> Payload);

public class QdrantClient(HttpClient http, string collectionName)
{
    private readonly string _path = $"/collections/{collectionName}";

    public async Task EnsureCollectionAsync(VectorConfig config) =>
        await http.PutAsJsonAsync(_path, new { vectors = new { config.Size, config.Distance }, on_disk_payload = true });

    public async Task EnsurePersonIdIndexAsync()
    {
        var res = await http.PutAsJsonAsync($"{_path}/index?wait=true", new { field_name = "personId", field_schema = "keyword" });
        if (!res.IsSuccessStatusCode) throw new Exception($"Index Failed: {await res.Content.ReadAsStringAsync()}");
    }

    public async Task WaitUntilCollectionExistsAsync(CancellationToken ct = default, int delay = 1000)
    {
        while (!ct.IsCancellationRequested)
        {
            var res = await http.GetAsync(_path, ct);
            if (res.IsSuccessStatusCode)
            {
                var node = await res.Content.ReadFromJsonAsync<JsonObject>(ct);
                if (node?["result"]?["status"]?.ToString() == "green") return;
            }
            await Task.Delay(delay, ct);
        }
    }

    public async Task UpsertAsync(string id, float[] vector, object payload) =>
        await http.PutAsJsonAsync($"{_path}/points?wait=true", new { points = new[] { new { id, vector, payload } } });

    public async Task<List<ScoredPoint>> SearchAsync(string personId, float[] vector, int topK)
    {
        var body = new
        {
            vector,
            limit = topK,
            with_payload = true,
            filter = new { must = new[] { new { key = "personId", match = new { value = personId } } } }
        };

        var res = await http.PostAsJsonAsync($"{_path}/points/search", body);
        var wrap = await res.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<QdrantResponse<List<SearchResult>>>();

        return [.. (wrap?.Result ?? []).Select(r => new ScoredPoint(r.Id?.ToString() ?? "", r.Score, r.Payload ?? []))];
    }

    private class SearchResult { public float Score { get; set; } public object? Id { get; set; } public Dictionary<string, object>? Payload { get; set; } }
    private class QdrantResponse<T> { public T? Result { get; set; } }
}
