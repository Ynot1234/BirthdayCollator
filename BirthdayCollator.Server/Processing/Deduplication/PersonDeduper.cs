using System.Text;

namespace BirthdayCollator.Server.Processing.Deduplication;

public sealed partial class PersonDeduper
{
    public List<Person> Deduplicate(List<Person> people)
    {
        if (people.Count <= 1) return people;

        Dictionary<string, Person> uniqueMap = [];

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

        if (string.IsNullOrWhiteSpace(name1) || string.IsNullOrWhiteSpace(name2))
            return false;

        if (string.Equals(name1, name2, StringComparison.OrdinalIgnoreCase))
            return true;

        var set1 = CanonicalTokens(name1).ToArray();
        var set2 = CanonicalTokens(name2).ToArray();

        if (set1.Length == 2 && set2.Length == 2)
        {
            return string.Equals(
                set1[1],
                set2[1],
                StringComparison.OrdinalIgnoreCase
            );
        }

        int overlap = set1
            .Intersect(set2, StringComparer.OrdinalIgnoreCase)
            .Count();

        return overlap >= 2;
    }


    private static string CanonicalNameKey(string name)
    {
        string clean = ExtractNameOnly(name);

        var tokens = CanonicalTokens(clean);
        return tokens.Length == 0 ? "unknown" : tokens[^1].ToLowerInvariant();
    }


    private static string ExtractNameOnly(string raw)
    {
        int cut = raw.IndexOfAny(['[', '(']);
        if (cut > 0)
            raw = raw[..cut];

        return raw.Trim();
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

        return p.SourceSlug switch
        {
            var s when s.Equals(Slugs.OnThisDay, StringComparison.OrdinalIgnoreCase) => 1,
            var s when s.Equals(Slugs.Imdb, StringComparison.OrdinalIgnoreCase) => 2,
            var s when s.Equals(Slugs.Genarians, StringComparison.OrdinalIgnoreCase) => 3,
            _ => 4
        };

    }
}