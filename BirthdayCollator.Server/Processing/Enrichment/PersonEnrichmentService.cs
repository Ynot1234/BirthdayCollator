using BirthdayCollator.Server.Models;
using Microsoft.Extensions.Caching.Memory;
using BirthdayCollator.Server.AI.Services;

namespace BirthdayCollator.Server.Processing.Enrichment;

public interface IPersonEnrichmentService
{
    Task<Person> EnrichAsync(Person person);
    Task<string> GetSummaryAsync(string name, string description);
}

public class PersonEnrichmentService(IAIService ai, IMemoryCache cache) : IPersonEnrichmentService
{
    public async Task<Person> EnrichAsync(Person person)
    {
        person.Summary = await GetSummaryAsync(person.Name, person.Description);
        return person;
    }

    public async Task<string> GetSummaryAsync(string name, string description)
    {
        string n = name.Trim();
        string d = description.Trim();
        string cacheKey = $"summary:{n}:{d.GetHashCode()}";

        return await cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.Priority = CacheItemPriority.NeverRemove;
            string raw = await ai.SummarizeAsync(n, d);

            int index = raw.IndexOf(n, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
                raw = raw.Remove(index, n.Length).Trim();

            return  PersonAIEnricher.NormalizeDescription(raw);
        }) ?? string.Empty;
    }

}
