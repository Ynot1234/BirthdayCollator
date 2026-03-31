using System.Text;
using System.Text.RegularExpressions;

namespace BirthdayCollator.Server.Processing.Deduplication;

public sealed partial class PersonDeduper
{
    public List<Person> Deduplicate(List<Person> people)
    {
        if (people.Count <= 1) return people;

        var uniqueMap = new Dictionary<string, Person>();

        foreach (var p in people)
        {
            string key = CanonicalNameKey(p.Name);

            if (!uniqueMap.TryGetValue(key, out var existing))
            {
                uniqueMap[key] = p;
                continue;
            }

            if (IsSamePerson(existing.Name, p.Name))
            {
                int scoreP = GetScore(p);
                int scoreExisting = GetScore(existing);

                bool isPBetter = scoreP > scoreExisting ||
                                (scoreP == scoreExisting && (p.Description?.Length ?? 0) > (existing.Description?.Length ?? 0));

                if (isPBetter)
                {
                    uniqueMap[key] = p;
                }
            }
            else
            {
                uniqueMap[$"{key}-{Guid.NewGuid()}"] = p;
            }
        }

        return [.. uniqueMap.Values];
    }

    private static bool IsSamePerson(string name1, string name2)
    {
        var set1 = CanonicalTokens(name1);
        var set2 = CanonicalTokens(name2);

        int overlap = set1.Intersect(set2, StringComparer.OrdinalIgnoreCase).Count();

        return overlap >= 2;
    }

    private static string CanonicalNameKey(string name)
    {
        var tokens = CanonicalTokens(name);
        return tokens.Length == 0 ? "unknown" : tokens[^1].ToLowerInvariant();
    }

    private static string[] CanonicalTokens(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return [];

        string clean = RegexPatterns.BoundaryMarkers().Replace(s, " ");
        clean = clean.Normalize(NormalizationForm.FormD);

        StringBuilder sb = new();
        foreach (char c in clean)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                {
                    sb.Append(c);
                }
            }
        }

        return [.. sb.ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(t => t.Length > 2)
            .Select(t => t.ToLowerInvariant())];
    }

    private static int GetScore(Person p)
    {
        if (string.IsNullOrEmpty(p.SourceSlug)) return 0;

        return p.SourceSlug.ToLowerInvariant() switch
        {
            "on-this-day" => 1,
            "imdb" => 2,
            "generarians" => 3,
            _ => 4
        };
    }
}