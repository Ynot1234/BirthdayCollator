using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Helpers;
using static BirthdayCollator.Server.Constants.AppStrings;

namespace BirthdayCollator.Server.Processing.Deduplication;

public sealed class PersonDeduper
{
    public List<Person> Deduplicate(List<Person> people)
    {
        if (people.Count <= 1) return people;

        List<Person> uniquePeople = [];

        foreach (var p in people)
        {
            string pName = StringNormalization.ToComparableSlug(p.Name);

            var existing = uniquePeople.FirstOrDefault(u =>
                u.BirthYear == p.BirthYear &&
                (pName.Contains(StringNormalization.ToComparableSlug(u.Name)) ||
                 StringNormalization.ToComparableSlug(u.Name).Contains(pName)));

            if (existing == null)
            {
                uniquePeople.Add(p);
                continue;
            }

            int scoreP = GetScore(p);
            int scoreExisting = GetScore(existing);

            if (scoreP > scoreExisting ||
               (scoreP == scoreExisting && (p.Description?.Length ?? 0) > (existing.Description?.Length ?? 0)))
            {
                uniquePeople.Remove(existing);
                uniquePeople.Add(p);
            }
        }

        return uniquePeople;
    }

    private static int GetScore(Person p)
    {
        if (string.IsNullOrEmpty(p.SourceSlug)) return 0;

        if (p.SourceSlug.Equals(Slugs.OnThisDay, StringComparison.OrdinalIgnoreCase))
            return 1;

        if (p.SourceSlug.Equals(Slugs.Imdb, StringComparison.OrdinalIgnoreCase))
            return 2;

        if (p.SourceSlug.Equals(Slugs.Genarians, StringComparison.OrdinalIgnoreCase))
            return 3;

        return 4;
    }
}