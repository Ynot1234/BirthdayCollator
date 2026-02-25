using BirthdayCollator.Helpers;
using BirthdayCollator.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BirthdayCollator.Processing
{
    public class DeduplicateByURL
    {
        public record NormalizedUrl(Person Person, string Url);
        public List<Person> DeduplicateByUrl(IEnumerable<Person> people)
        {
            List<Person> withUrls = [.. people.Where(p => !string.IsNullOrWhiteSpace(p.Url))];

            List<NormalizedUrl> normalized = [.. withUrls.Select(p => new NormalizedUrl(p, UrlNomalization.NormalizeUrl(p.Url)))];

            var grouped = normalized
                .GroupBy(x => x.Url, StringComparer.OrdinalIgnoreCase)
                .ToList();

            List<Person> deduped = [.. grouped.Select(g => g.First().Person)];

            return deduped;
        }
    }
}
