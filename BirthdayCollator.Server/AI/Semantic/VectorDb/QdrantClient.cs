using System.Net;
using static System.Net.WebRequestMethods;

namespace BirthdayCollator.Server.AI.Semantic.VectorDb;

public class QdrantClient(HttpClient http)
{
    private const string CollectionName = "birthdaycollator";

    public async Task EnsureCollectionAsync(int vectorSize)
    {
        var body = new
        {
            vectors = new
            {
                size = vectorSize,
                distance = "Cosine"
            },
            on_disk_payload = true
        };

        await http.PutAsJsonAsync($"/collections/{CollectionName}", body);
    }

    public async Task EnsurePersonIdIndexAsync()
    {
        var body = new
        {
            field_name = "personId",
            field_schema = "keyword"
        };

        // FIX: Must use PUT for this endpoint
        var response = await http.PutAsJsonAsync(
            $"/collections/{CollectionName}/index?wait=true",
            body
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Index Creation Failed: {error}");
        }
    }

    public async Task WaitUntilCollectionExistsAsync()
    {
        while (true)
        {
            try
            {
                // Specifically check for the 'status' property in the response
                var response = await http.GetFromJsonAsync<System.Text.Json.Nodes.JsonObject>($"/collections/{CollectionName}");
                var status = response?["result"]?["status"]?.ToString();

                if (status == "green")
                {
                    Console.WriteLine("Collection is GREEN and ready.");
                    return;
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // This is expected while the cloud cluster is synchronizing
                Console.WriteLine("Collection not found yet by this node...");
            }

            await Task.Delay(1000);
        }
    }


    public async Task UpsertAsync(string id, float[] vector, object payload)
    {
        // Qdrant requires UUID or integer IDs
        var pointId = Guid.TryParse(id, out var parsed)
            ? id
            : Guid.NewGuid().ToString();

        var body = new
        {
            ids = new[] { pointId },
            vectors = new[] { vector },
            payloads = new[] { payload }
        };

        var response = await http.PostAsJsonAsync(
            $"/collections/{CollectionName}/points",
            body
        );

        var error = await response.Content.ReadAsStringAsync();
        Console.WriteLine("UPSERT RESPONSE:");
        Console.WriteLine(error);

        response.EnsureSuccessStatusCode();
    }


    public async Task<List<(string Text, float Score)>> SearchAsync(
    string personId,
    float[] vector,
    int topK)
    {
        var body = new
        {
            vector,
            limit = topK,
            with_payload = true, // Ensure the payload is actually returned
            filter = new
            {
                must = new[]
                {
                new { key = "personId", match = new { value = personId } }
            }
            }
        };

        var response = await http.PostAsJsonAsync($"/collections/{CollectionName}/points/search", body);
        response.EnsureSuccessStatusCode();

        var wrapper = await response.Content.ReadFromJsonAsync<QdrantResponse<List<SearchResult>>>();

        if (wrapper?.Result == null) return [];

        var list = new List<(string Text, float Score)>();

        foreach (var item in wrapper.Result)
        {
            string textValue = "N/A";

            if (item.Payload != null && item.Payload.TryGetValue("text", out var val))
            {
                textValue = val?.ToString() ?? "N/A";
            }

            list.Add((textValue, item.Score));
        }

        return list;

    }


}
public class SearchResult
{
    public float Score { get; set; }
    // Qdrant returns 'id' as well, which is often useful
    public object Id { get; set; } = default!;
    public Dictionary<string, object> Payload { get; set; } = default!;
}
