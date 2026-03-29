using BirthdayCollator.Server.AI.Services;

namespace BirthdayCollator.Server.Processing.Enrichment;

public sealed class PersonAIEnricher(IBioService ai)
{
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



    public async Task EnrichPeopleAsync(List<Person> people, string? apiKey = null)
    {
        var candidates = people.Where(p => p.BirthYear == 1).ToList();

        if (candidates.Count == 0) return;

        var tasks = candidates.Select(async person =>
        {
            string result = await ai.ExtractBirthYearAsync(person.Name, person.Description, apiKey);

            if (int.TryParse(result, out int validYear) && validYear > 1)
            {
                person.BirthYear = validYear;
            }
        });

        await Task.WhenAll(tasks);
    }
}