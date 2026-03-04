using BirthdayCollator.Server.Models;
using Microsoft.Extensions.Caching.Memory;
using BirthdayCollator.Server.AI.Services;

namespace BirthdayCollator.Server.Processing.Enrichment;

public interface IPersonEnrichmentService
{
    Task<Person> EnrichAsync(Person person, string? apiKey = null);
    Task<string> GetSummaryAsync(string name, string description, string? apiKey = null);
}

public class PersonEnrichmentService : IPersonEnrichmentService
{
    private readonly IAIService ai;
    private readonly IMemoryCache cache;
    private readonly IConfiguration config;

    public PersonEnrichmentService(IAIService ai, IMemoryCache cache, IConfiguration config)
    {
        this.ai = ai;
        this.cache = cache;
        this.config = config;
    }

    public async Task<Person> EnrichAsync(Person person, string? apiKey = null)
    {
        person.Summary = await GetSummaryAsync(person.Name, person.Description, apiKey);
        return person;
    }

    public async Task<string> GetSummaryAsync(string name, string description, string? apiKey = null)
    {
        string n = name.Trim();
        string d = description.Trim();
        string cacheKey = $"summary:{n}:{d.GetHashCode()}";

        // 1. Prefer user-supplied key (production)
        var key = apiKey;

        // 2. Fall back to server key (dev via User Secrets)
        if (string.IsNullOrWhiteSpace(key))
        {
            key = config["OpenAI:ApiKey"];
        }

        // 3. If still no key → return safe message (prevents HTML fallback)
        if (string.IsNullOrWhiteSpace(key))
        {
            return "No OpenAI API key provided.";
        }

        return await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.Priority = CacheItemPriority.NeverRemove;

            string raw = await ai.SummarizeAsync(n, d, key);

            int index = raw.IndexOf(n, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
                raw = raw.Remove(index, n.Length).Trim();

            return PersonAIEnricher.NormalizeDescription(raw);
        }) ?? string.Empty;
    }
}
