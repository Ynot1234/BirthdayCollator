using BirthdayCollator.Server.AI.Services;

namespace BirthdayCollator.Server.Processing.Enrichment;

public sealed class PersonAIEnricher(IBioService ai, IConfiguration config)
{
    private readonly Dictionary<string, int> _birthYearCache = [];


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
    public async Task EnrichAndFilterPeopleAsync(List<Person> people, string? apiKey = null)
    {
        apiKey ??= config["OpenAI:ApiKey"];

        foreach (var person in people)
        {
            if (_birthYearCache.TryGetValue(person.Name, out int cachedYear))
            {
                person.BirthYear = cachedYear;
                continue;
            }

            if (person.BirthYear == 1)
            {
                string result = await ai.ExtractBirthYearAsync(person.Name, person.Description, apiKey);

                if (int.TryParse(result, out int validYear) && validYear > 1)
                {
                    person.BirthYear = validYear;
                    _birthYearCache[person.Name] = validYear; 
                }
                else
                {
                    person.BirthYear = 2; 
                    _birthYearCache[person.Name] = 2; 
                }
            }
        }

        people.RemoveAll(p => p.BirthYear == 1 || p.BirthYear == 2);
    }

}