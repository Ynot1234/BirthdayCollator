namespace BirthdayCollator.Server.AI.Services
{
    public interface IBioService
    {
        Task<string> SummarizeAsync(string name, string desc, string? apiKey = null);
    }
}
