using System.Text;
using System.Text.RegularExpressions;

namespace BirthdayCollator.Server.Processing.Deduplication;

public sealed class PersonDeduper
{
    public List<Person> Deduplicate(List<Person> people)
    {
        if (people.Count <= 1)
            return people;

        var uniqueMap = new Dictionary<string, Person>();

        foreach (var p in people)
        {
            string key = CanonicalNameKey(p.Name);

            if (!uniqueMap.TryGetValue(key, out var existing))
            {
                uniqueMap[key] = p;
                continue;
            }

            if (!HasTwoWordOverlap(existing.Name, p.Name))
            {
                uniqueMap[$"{key}-{Guid.NewGuid()}"] = p;
                continue;
            }

            int scoreP = GetScore(p);
            int scoreExisting = GetScore(existing);

            bool isPBetter =
                scoreP > scoreExisting ||
                (scoreP == scoreExisting &&
                 (p.Description?.Length ?? 0) > (existing.Description?.Length ?? 0));

            if (isPBetter)
                uniqueMap[key] = p;
        }

        return [.. uniqueMap.Values];
    }


    private static string CanonicalNameKey(string name)
    {
        string cleanName = ExtractNameOnly(name);

        var tokens = CanonicalTokens(cleanName);
        return tokens.Length == 0 ? "" : tokens[^1];
    }


    private static string ExtractNameOnly(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return raw;

        var sb = new StringBuilder();

        foreach (char c in raw)
        {
            if (char.IsLetter(c) || c == ' ')
            {
                sb.Append(c);
            }
            else
            {
                break;
            }
        }

        return sb.ToString().Trim();
    }



    private static bool HasTwoWordOverlap(string a, string b)
    {
        var aTokens = CanonicalTokens(a);
        var bTokens = CanonicalTokens(b);

        return aTokens.Intersect(bTokens).Count() >= 2;
    }



    private static string[] CanonicalTokens(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return Array.Empty<string>();

        s = Regex.Replace(s, @"(?<=\b\p{L})\.(?=\p{L}\b)", " ");

        s = Regex.Replace(s, "[\"'()]", "");
        s = Regex.Replace(s, @"[^\p{L}\p{N}\s]", "");

        s = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in s)
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        s = sb.ToString();

        return [.. Regex.Split(s.ToLowerInvariant(), @"\s+").Where(t => t.Length > 0)];
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