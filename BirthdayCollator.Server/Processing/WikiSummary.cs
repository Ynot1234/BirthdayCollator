using System.Text.Json.Serialization;

namespace BirthdayCollator.Processing
{
    public class WikiSummary
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Extract { get; set; }

        [JsonPropertyName("extract_html")]
        public string? ExtractHtml { get; set; }

        public Thumbnail? Thumbnail { get; set; }

        [JsonPropertyName("originalimage")]
        public Thumbnail? OriginalImage { get; set; }

    }

}
