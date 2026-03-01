using BirthdayCollator.Server.Models;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BirthdayCollator.Server.Processing.Deduplication
{
    public class PersonDeduper()
    {
        public List<Person> DeduplicateByNameAndYear(List<Person> people)
        {
            HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
            List<Person> final = new();

            foreach (Person p in people)
            {
                // Step 1: build normalized key
                string normalizedName = NormalizeName(p.Name);
                int year = p.BirthDate.Year;
                string key = $"{normalizedName}|{year}";

                // Step 2: if new key, add immediately
                if (seen.Add(key))
                {
                    final.Add(p);
                    continue;
                }

                // Step 3: find existing entry using the SAME normalization rules
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

                // Step 4: defensive fallback — should never happen if normalization matches
                if (existing is null)
                {
                    final.Add(p);
                    continue;
                }

                // Step 5: apply your OTD replacement rule
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

            // Remove bracketed metadata
            int bracketIndex = name.IndexOf('[');
            if (bracketIndex >= 0)
                name = name[..bracketIndex];

            // Normalize Unicode (NFD)
            string normalized = name.Normalize(NormalizationForm.FormD);

            // Strip diacritics
            var sb = new StringBuilder();
            foreach (char c in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            normalized = sb.ToString();

            // Remove punctuation
            normalized = new string(normalized.Where(c => !char.IsPunctuation(c)).ToArray());

            // Collapse whitespace
            normalized = Regex.Replace(normalized, @"\s+", " ");

            normalized = normalized.Replace(" ", "");


            // Lowercase + trim
            return normalized.Trim().ToLowerInvariant();
        }



        private static bool IsOnThisDay(Person p) => string.Equals(p.DisplaySlug, "OnThisDay", StringComparison.OrdinalIgnoreCase);

    }



}