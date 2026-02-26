using BirthdayCollator.Server.AI.Services;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Constants;

namespace BirthdayCollator.Server.Processing.Enrichment;

public sealed class PersonAIEnricher(IAIService ai)
{
    public async Task<Person> EnrichAsync(Person person)
    {
        string summary = await ai.SummarizeAsync(person.Name, person.Description);

        int index = summary.IndexOf(person.Name);

        if (index >= 0)
        {
            summary = summary.Remove(index, person.Name.Length).Trim();
        }

        summary = NormalizeDescription(summary);
        person.Summary = summary;
        return person;
    }

    public async Task<List<Person>> EnrichPeopleAsync(List<Person> people)
    {
        var tasks = people.Select(p => EnrichAsync(p));
        return [.. await Task.WhenAll(tasks)];
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