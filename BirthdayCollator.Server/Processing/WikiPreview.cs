namespace BirthdayCollator.Processing
{
    public class WikiPreview
    {
        public bool IsLoading { get; set; }
        public string? Html { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Error { get; set; }

        public bool FlipUp { get; set; }  
    }

}
