using BirthdayCollator.Server.Models;
using BirthdayCollator.Helpers;
using System.Globalization;
using System.Text;

namespace BirthdayCollator.Server.Processing.Deduplication
{
    public partial class PersonDeduper()
    {
        public List<Person> DeduplicateByNameAndYear(List<Person> people)
        {
            HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
            List<Person> final = [];

            foreach (Person p in people)
            {
                string normalizedName = NormalizeName(p.Name);
                int year = p.BirthDate.Year;
                string key = $"{normalizedName}|{year}";

                if (seen.Add(key))
                {
                    final.Add(p);
                    continue;
                }

                Person? existing = null;

                foreach (var x in final)
                {
                    string existingName = NormalizeName(x.Name);
                    int existingYear = x.BirthDate.Year;

                    bool sameName = existingName.Equals(normalizedName, StringComparison.OrdinalIgnoreCase);
                    bool sameYear = existingYear == year;

                    if (sameName && sameYear)
                    {
                        existing = x;
                        break;
                    }
                }

                if (existing is null)
                {
                    final.Add(p);
                    continue;
                }

                bool existingIsOTD = IsOnThisDay(existing);
                bool incomingIsOTD = IsOnThisDay(p);

                if (existingIsOTD && !incomingIsOTD)
                {
                    final.Remove(existing);
                    final.Add(p);
                }
            }

            return final;
        }



        public static string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            int bracketIndex = name.IndexOf('[');
            if (bracketIndex >= 0)
                name = name[..bracketIndex];

            string normalized = name.Normalize(NormalizationForm.FormD);

            var sb = new StringBuilder();
            foreach (char c in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            normalized = sb.ToString();
            normalized = new string([.. normalized.Where(c => !char.IsPunctuation(c))]);
            normalized = RegexPatterns.NormalizeWhitespace().Replace(normalized, " ");
            normalized = normalized.Replace(" ", "");
            return normalized.Trim().ToLowerInvariant();
        }

        private static bool IsOnThisDay(Person p) => string.Equals(p.DisplaySlug, "OnThisDay", StringComparison.OrdinalIgnoreCase);
    }
}