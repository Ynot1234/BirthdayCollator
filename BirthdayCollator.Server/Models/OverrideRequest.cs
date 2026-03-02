namespace BirthdayCollator.Server.Models
{
    public sealed class OverrideRequest
    {
        public string? Year { get; set; }
        public bool IncludeAll { get; set; }
    }
}
