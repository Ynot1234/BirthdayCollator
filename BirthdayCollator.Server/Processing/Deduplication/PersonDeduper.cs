using System.Text;
using System.Globalization;

namespace BirthdayCollator.Server.Processing.Deduplication;

public sealed partial class PersonDeduper
{
    public List<Person> Deduplicate(List<Person> people)
    {
        if (people.Count <= 1)
            return people;

        List<Person> result = [];

        foreach (var p in people)
        {
            Person? bestMatch = null;
            int bestIndex = -1;

            for (int i = 0; i < result.Count; i++)
            {
                var existing = result[i];

                if (IsSamePerson(existing.Name, p.Name))
                {
                    bestMatch = existing;
                    bestIndex = i;
                    break;
                }
            }

            if (bestMatch is null)
            {
                result.Add(p);
            }
            else
            {
                int scoreP = GetScore(p);
                int scoreExisting = GetScore(bestMatch);

                bool isPBetter =
                    scoreP > scoreExisting ||
                    (scoreP == scoreExisting &&
                     (p.Description?.Length ?? 0) > (bestMatch.Description?.Length ?? 0));

                if (isPBetter)
                    result[bestIndex] = p;
            }
        }

        return result;
    }

    private static bool IsSamePerson(string name1, string name2)
    {
        if (string.IsNullOrWhiteSpace(name1) || string.IsNullOrWhiteSpace(name2))
            return false;

        if (string.Equals(name1, name2, StringComparison.OrdinalIgnoreCase))
            return true;

        var t1 = CanonicalTokens(name1);
        var t2 = CanonicalTokens(name2);

        if (t1.Length == 0 || t2.Length == 0)
            return false;

        var set1 = t1.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var set2 = t2.ToHashSet(StringComparer.OrdinalIgnoreCase);

        int overlap = set1.Intersect(set2, StringComparer.OrdinalIgnoreCase).Count();
        if (overlap >= 2)
            return true;

        string s1 = t1[^1];
        string s2 = t2[^1];

        if (!string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase))
            return false;

        string f1 = t1[0];
        string f2 = t2[0];

        if (f1.StartsWith(f2, StringComparison.OrdinalIgnoreCase) ||
            f2.StartsWith(f1, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
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
        if (string.IsNullOrWhiteSpace(s))
            return [];

        string clean = RegexPatterns.BoundaryMarkers().Replace(s, " ");
        clean = clean.Normalize(NormalizationForm.FormD);

        StringBuilder sb = new();
        foreach (char c in clean)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                    sb.Append(c);
            }
        }

        return sb.ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(t => t.Length > 2)
            .Select(t => t.ToLowerInvariant())
            .ToArray();
    }

    private static int GetScore(Person p)
    {
        if (string.IsNullOrEmpty(p.SourceSlug))
            return 0;

        return p.SourceSlug switch
        {
            var s when s.Equals(Slugs.OnThisDay, StringComparison.OrdinalIgnoreCase) => 1,
            var s when s.Equals(Slugs.Imdb, StringComparison.OrdinalIgnoreCase) => 2,
            var s when s.Equals(Slugs.Genarians, StringComparison.OrdinalIgnoreCase) => 3,
            _ => 4
        };
    }
}
