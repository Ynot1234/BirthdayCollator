using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Helpers;
using static BirthdayCollator.Server.Constants.AppStrings;

namespace BirthdayCollator.Server.Processing.Deduplication;

public sealed class PersonDeduper
{
    public List<Person> Deduplicate(List<Person> people)
    {
        if (people.Count <= 1) return people;

        var uniquePeople = new Dictionary<string, Person>(StringComparer.OrdinalIgnoreCase);

        foreach (var p in people)
        {
            var key = $"{StringNormalization.ToComparableSlug(p.Name)}|{p.BirthYear}";

            if (!uniquePeople.TryGetValue(key, out var existing))
            {
                uniquePeople[key] = p;
                continue;
            }

            int scoreP = GetScore(p);
            int scoreExisting = GetScore(existing);

            if (scoreP > scoreExisting ||
               (scoreP == scoreExisting &&
                (p.Description?.Length ?? 0) > (existing.Description?.Length ?? 0)))
            {
                uniquePeople[key] = p;
            }
        }

        return [.. uniquePeople.Values];
    }

    private static int GetScore(Person p)
    {
        if (string.IsNullOrEmpty(p.SourceSlug)) return 0;

        if (p.SourceSlug.Equals(Slugs.OnThisDay, StringComparison.OrdinalIgnoreCase))
            return 1;

        if (p.SourceSlug.Equals(Slugs.Genarians, StringComparison.OrdinalIgnoreCase))
            return 2;

        return 3;
    }
}