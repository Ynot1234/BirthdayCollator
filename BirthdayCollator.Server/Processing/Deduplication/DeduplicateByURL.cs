using BirthdayCollator.Server.Helpers;
using BirthdayCollator.Server.Models;

namespace BirthdayCollator.Server.Processing.Deduplication
{
    public class DeduplicateByURL
    {
        public record NormalizedUrl(Person Person, string Url);

        public List<Person> DeduplicateByUrl(IEnumerable<Person> people)
        {
            List<Person> withUrls = [.. people.Where(p => !string.IsNullOrWhiteSpace(p.Url))];
            List<Person> withoutUrls = [.. people.Where(p => string.IsNullOrWhiteSpace(p.Url))];
            List<NormalizedUrl> normalized = [.. withUrls.Select(p => new NormalizedUrl(p, UrlNomalization.NormalizeUrl(p.Url!)))];
            var grouped = normalized.GroupBy(x => x.Url, StringComparer.OrdinalIgnoreCase);
            List<Person> dedupedWithUrls = [.. grouped.Select(g => g.First().Person)];
            dedupedWithUrls.AddRange(withoutUrls);
            return dedupedWithUrls;
        }
    }
}
