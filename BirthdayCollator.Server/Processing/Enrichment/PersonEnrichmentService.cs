using BirthdayCollator.Server.Models;
using Microsoft.Extensions.Caching.Memory;
using BirthdayCollator.Server.AI.Services;

namespace BirthdayCollator.Server.Processing.Enrichment;

public interface IPersonEnrichmentService
{
    Task<Person> EnrichAsync(Person person, string? apiKey = null);
    Task<string> GetSummaryAsync(string name, string description, string? apiKey = null);
}

public class PersonEnrichmentService(IAIService ai, IMemoryCache cache, IConfiguration config) : IPersonEnrichmentService
{
    public async Task<Person> EnrichAsync(Person person, string? apiKey = null)
    {
        person.Summary = await GetSummaryAsync(person.Name, person.Description, apiKey);
        return person;
    }


    private async Task<string> GenerateSummaryAsync(string name, string description, string apiKey)
    {
        string raw = await ai.SummarizeAsync(name, description, apiKey);

        int index = raw.IndexOf(name, StringComparison.OrdinalIgnoreCase);

        if (index >= 0)
            raw = raw.Remove(index, name.Length).Trim();

        return PersonAIEnricher.NormalizeDescription(raw);
    }

    public async Task<string> GetSummaryAsync(string name, string description, string? apiKey = null)
    {
        string n = name.Trim();
        string d = description.Trim();
        string cacheKey = $"summary:{n}:{d.GetHashCode()}";

        var key = apiKey ?? config["OpenAI:ApiKey"];

        if (string.IsNullOrWhiteSpace(key))
            return "No OpenAI API key provided.";

        return await CreateCachedSummaryAsync(cacheKey, n, d, key);
    }

    private async Task<string> CreateCachedSummaryAsync(string cacheKey, 
                                                        string name, 
                                                        string description, 
                                                        string apiKey)
    {
        var summary = await cache.GetOrCreateAsync(
            cacheKey,
            entry => CreateSummaryValueAsync(entry, name, description, apiKey)
        );

        return summary ?? string.Empty;
    }


    private async Task<string> CreateSummaryValueAsync(ICacheEntry entry,   
                                                       string name, 
                                                       string description, 
                                                       string apiKey)
    {
        entry.Priority = CacheItemPriority.NeverRemove;
        return await GenerateSummaryAsync(name, description, apiKey);
    }
}