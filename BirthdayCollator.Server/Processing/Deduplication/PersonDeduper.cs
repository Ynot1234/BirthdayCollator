using BirthdayCollator.Server.Helpers;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Html;

namespace BirthdayCollator.Server.Processing.Deduplication;
public sealed class PersonDeduper
{
    public List<Person> DeduplicateByNameAndYear(List<Person> people)
    {
        var uniquePeople = new Dictionary<string, Person>(StringComparer.OrdinalIgnoreCase);

        foreach (var p in people)
        {
            var key = $"{UrlNormalization.ToComparableSlug(p.Name)}|{p.BirthYear}";

            if (!uniquePeople.TryGetValue(key, out var existing))
            {
                uniquePeople[key] = p;
                continue;
            }

            if (IsOnThisDay(existing) && !IsOnThisDay(p))
            {
                uniquePeople[key] = p;
            }
        }

        return [.. uniquePeople.Values];
    }

    private static bool IsOnThisDay(Person p) => string.Equals(p.DisplaySlug, "OnThisDay", StringComparison.OrdinalIgnoreCase);
}
