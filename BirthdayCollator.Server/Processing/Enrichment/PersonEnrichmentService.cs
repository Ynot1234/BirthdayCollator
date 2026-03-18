using BirthdayCollator.Server.AI.Services;
using Microsoft.Extensions.Caching.Hybrid;

namespace BirthdayCollator.Server.Processing.Enrichment;

public interface IPersonEnrichmentService
{
    Task<Person> EnrichAsync(Person person, string? apiKey = null);
    Task<string> GetSummaryAsync(string name, string description, string? apiKey = null);
}

public class PersonEnrichmentService(
    IBioService ai,
    HybridCache cache,
    IConfiguration config) : IPersonEnrichmentService
{
    public async Task<Person> EnrichAsync(Person person, string? apiKey = null)
    {
        if (!string.IsNullOrWhiteSpace(person.Summary))
            return person;

        person.Summary = await GetSummaryAsync(person.Name, person.Description, apiKey);
        return person;
    }

    public async Task<string> GetSummaryAsync(string name, string desc, string? apiKey = null)
    {
        var key = apiKey ?? config["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(key)) return "Missing API Key";

        string cacheKey = $"sum:{name.Trim().ToLowerInvariant()}:{desc.Length}";

        return await cache.GetOrCreateAsync(cacheKey, async token =>
        {
            string raw = await ai.SummarizeAsync(name, desc, key);
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

            int index = raw.IndexOf(name, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                ReadOnlySpan<char> rawSpan = raw.AsSpan();
                raw = string.Concat(rawSpan[..index], rawSpan[(index + name.Length)..]).Trim();
            }

            return WikiTextUtility.NormalizeDescription(raw);
        });
    }
}