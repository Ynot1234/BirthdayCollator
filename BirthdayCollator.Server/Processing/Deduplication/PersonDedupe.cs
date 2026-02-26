using BirthdayCollator.Server.Models;

namespace BirthdayCollator.Server.Processing.Deduplication
{
    public  class PersonDedupe
    {
        public List<Person> DedupePeople(List<Person> people)
        {
            List<Person> final = [];

            foreach (var p in people)
            {
                var existing = final.FirstOrDefault(x =>
                    x.BirthYear == p.BirthYear &&
                    FirstName(x.Name) == FirstName(p.Name) &&
                    HasKeywordOverlap(x.Description, p.Description)
                );

                if (existing is null)
                {
                    final.Add(p);
                }
                else
                {
                    if (Score(p) > Score(existing))
                    {
                        final.Remove(existing);
                        final.Add(p);
                    }
                }
            }

            return final;
        }

        private static string FirstName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            return name.Split(' ')[0].Trim().ToLowerInvariant();
        }

        private static readonly HashSet<string> StopWords =
         [
             "of", "in", "the", "and", "to", "for", "on", "at", "by", "a", "an"
         ];

        private static HashSet<string> ExtractKeywords(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return [];

            return [.. text
                .ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim(',', '.', ';', ':', '!', '?', '(', ')', '[', ']'))
                .Where(w => w.Length >= 3 && !StopWords.Contains(w))];
        }


        private static bool HasKeywordOverlap(string? a, string? b)
        {
            var wa = ExtractKeywords(a);
            var wb = ExtractKeywords(b);

            int count = wa.Intersect(wb).Count();

            return count >= 2;
        }

        private static int Score(Person p)
        {
            var src = p.SourceUrl ?? string.Empty;

            if (src.Contains("Wiki", StringComparison.OrdinalIgnoreCase))
                return 3;

            if (src.Contains("Genarians", StringComparison.OrdinalIgnoreCase))
                return 2;

            if (src.Contains("OnThisDay", StringComparison.OrdinalIgnoreCase))
                return 1;

            return 0;
        }



    }

}
