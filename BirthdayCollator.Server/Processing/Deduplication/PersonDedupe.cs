using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Html;

namespace BirthdayCollator.Server.Processing.Deduplication;

public sealed class PersonDedupe
{
    public List<Person> DedupePeople(List<Person> people)
    {
        var groups = people.GroupBy(p => $"{p.BirthYear}|{WikiTextUtility.GetFirstName(p.Name)}");
        List<Person> final = [];

        foreach (var group in groups)
        {
            var candidates = group.ToList();

            while (candidates.Count > 0)
            {
                var current = candidates[0];
                candidates.RemoveAt(0);

                var duplicate = candidates.FirstOrDefault(other =>
                    AreLikelySamePerson(current, other));

                if (duplicate != null)
                {
                    var best = GetScore(current) >= GetScore(duplicate)
                        ? current
                        : duplicate;

                    final.Add(best);
                    candidates.Remove(duplicate);
                }
                else
                {
                    final.Add(current);
                }
            }
        }

        return final;
    }

    private static bool AreLikelySamePerson(Person a, Person b)
    {
        return WikiTextUtility.HasKeywordOverlap(a.Description, b.Description);
    }

    private static int GetScore(Person p) => p.SourceSlug switch
    {
        "Wikipedia" => 3,
        "Genarians" => 2,
        "OnThisDay" => 1,
        _ => 0
    };
}
