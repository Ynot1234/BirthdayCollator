namespace BirthdayCollator.Server.Processing.Deduplication;

public sealed class PersonDeduper
{
    public List<Person> Deduplicate(List<Person> people)
    {
        if (people.Count <= 1) return people;

        var uniqueMap = new Dictionary<(int Year, string Slug), Person>();

        foreach (var p in people)
        {
            int year = p.BirthYear;
            string slug = StringNormalization.ToComparableSlug(p.Name);
            var key = (year, slug);

            if (!uniqueMap.TryGetValue(key, out var existing))
            {
                uniqueMap[key] = p;
                continue;
            }

            int scoreP = GetScore(p);
            int scoreExisting = GetScore(existing);

            bool isPBetter = scoreP > scoreExisting ||
                            (scoreP == scoreExisting && (p.Description?.Length ?? 0) > (existing.Description?.Length ?? 0));

            if (isPBetter)
            {
                uniqueMap[key] = p;
            }
        }

        return [.. uniqueMap.Values];
    }

    private static int GetScore(Person p)
    {
        // Simple null check
        if (string.IsNullOrEmpty(p.SourceSlug)) return 0;

        // Direct string comparison in the switch is highly optimized in .NET 9
        return p.SourceSlug switch
        {
            var s when s.Equals(Slugs.OnThisDay, StringComparison.OrdinalIgnoreCase) => 1,
            var s when s.Equals(Slugs.Imdb, StringComparison.OrdinalIgnoreCase) => 2,
            var s when s.Equals(Slugs.Genarians, StringComparison.OrdinalIgnoreCase) => 3,
            _ => 4
        };
    }
}