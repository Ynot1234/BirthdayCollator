using BirthdayCollator.Server.AI.Services;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Constants;

namespace BirthdayCollator.Server.Processing.Enrichment;

public sealed class PersonAIEnricher
{
    private readonly IAIService ai;
    private readonly IConfiguration config;

    public PersonAIEnricher(IAIService ai, IConfiguration config)
    {
        this.ai = ai;
        this.config = config;
    }

    public async Task<Person> EnrichAsync(Person person, string? apiKey = null)
    {
        string summary = await SummarizeInternalAsync(person.Name, person.Description, apiKey);

        int index = summary.IndexOf(person.Name, StringComparison.OrdinalIgnoreCase);
        if (index >= 0)
        {
            summary = summary.Remove(index, person.Name.Length).Trim();
        }

        person.Summary = NormalizeDescription(summary);
        return person;
    }

    public async Task<List<Person>> EnrichPeopleAsync(List<Person> people, string? apiKey = null)
    {
        var tasks = people.Select(p => EnrichAsync(p, apiKey));
        return [.. await Task.WhenAll(tasks)];
    }

    private async Task<string> SummarizeInternalAsync(string name, string description, string? apiKey)
    {
        // 1. Prefer user-supplied key (production)
        var key = apiKey;

        // 2. Fall back to server key (dev via User Secrets)
        if (string.IsNullOrWhiteSpace(key))
        {
            key = config["OpenAI:ApiKey"];
        }

        // 3. If still no key → return a safe error instead of throwing HTML
        if (string.IsNullOrWhiteSpace(key))
        {
            return "No OpenAI API key provided.";
        }

        return await ai.SummarizeAsync(name, description, key);
    }

    public static string NormalizeDescription(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        foreach (var prefix in NameParsing.Prefixes)
        {
            if (text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                text = text[prefix.Length..].TrimStart();
                break;
            }
        }

        if (text.Length > 0)
            text = char.ToUpper(text[0]) + text.Substring(1);

        return text;
    }
}
