using BirthdayCollator.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BirthdayCollator.Processing
{
    public class PersonDeduper()
    {
        public List<Person> DeduplicateByNameAndYear(List<Person> people)
        {
            HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
            List<Person> final = [];

            foreach (Person p in people)
            {
                string key = $"{p.Name.Trim().ToLowerInvariant()}|{p.BirthDate.Year}";

                if (seen.Add(key))
                {
                    final.Add(p);
                }
                else
                {
                    // Duplicate detected — decide whether to replace
                    var existing = final.First(x =>
                        x.Name.Trim().Equals(p.Name.Trim(), StringComparison.OrdinalIgnoreCase) &&
                        x.BirthDate.Year == p.BirthDate.Year);

                    bool existingIsOTD = IsOnThisDay(existing);
                    bool incomingIsOTD = IsOnThisDay(p);

                    // Prefer non-OnThisDay entries
                    if (existingIsOTD && !incomingIsOTD)
                    {
                        final.Remove(existing);
                        final.Add(p);
                    }
                }
            }

            return final;
        }

        private static bool IsOnThisDay(Person p) => string.Equals(p.DisplaySlug, "OnThisDay", StringComparison.OrdinalIgnoreCase);

    }
}