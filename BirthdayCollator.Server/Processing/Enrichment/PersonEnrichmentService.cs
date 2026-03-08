using BirthdayCollator.Server.AI.Services;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Html;
using Microsoft.Extensions.Caching.Memory;

namespace BirthdayCollator.Server.Processing.Enrichment;

public interface IPersonEnrichmentService
{
    Task<Person> EnrichAsync(Person person, string? apiKey = null);
    Task<string> GetSummaryAsync(string name, string description, string? apiKey = null);
}

public class PersonEnrichmentService(
    IBioService ai, // Swapped to your custom interface
    IMemoryCache cache,
    IConfiguration config) : IPersonEnrichmentService
{
    public async Task<Person> EnrichAsync(Person person, string? apiKey = null)
    {
        person.Summary = await GetSummaryAsync(person.Name, person.Description, apiKey);
        return person;
    }

    public async Task<string> GetSummaryAsync(string name, string desc, string? apiKey = null)
    {
        var key = apiKey ?? config["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(key)) return "No API key provided.";

        string cacheKey = $"summary:{name.Trim()}:{desc.Trim().GetHashCode()}";

        return await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.Priority = CacheItemPriority.NeverRemove;
            string raw = await ai.SummarizeAsync(name, desc, key);
            int index = raw.IndexOf(name, StringComparison.OrdinalIgnoreCase);
            if (index >= 0) raw = raw.Remove(index, name.Length).Trim();
            return WikiTextUtility.NormalizeDescription(raw);
        }) ?? string.Empty;
    }
}
