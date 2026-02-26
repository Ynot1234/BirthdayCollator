using System.Text.Json.Serialization;

namespace BirthdayCollator.Server.Processing.Enrichment
{
    public class Thumbnail
    {
        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }
    }


}
