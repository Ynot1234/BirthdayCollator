namespace BirthdayCollator.Server.Processing.Deduplication;

public class NearDuplicateRemover
{
    private static string NormalizeWikiUrl(Person p)
    {
        if (string.IsNullOrWhiteSpace(p.Url))
            return $"no-url:{p.Name.ToLowerInvariant().Trim()}:{p.BirthYear}";

        return p.Url.AsSpan()
            .Trim()
            .ToString()
            .ToLowerInvariant()
            .Replace("_,jr.", "_jr.")
            .Replace("jr.", "jr")
            .Replace(",", "")
            .Replace("_", " "); 
    }

    public List<Person> RemoveNearDuplicates(List<Person> people)
    {
        if (people.Count == 0) return [];

        return [.. people
            .GroupBy(NormalizeWikiUrl)
            .Select(group => group
                .OrderBy(p => GetSourcePriority(p.Url))
                .First())];
    }

    private static int GetSourcePriority(string? url)
    {
        if (string.IsNullOrEmpty(url)) return 10;

        var span = url.AsSpan();

        if (span.Contains(Slugs.Wikipedia.AsSpan(), StringComparison.OrdinalIgnoreCase)) return 0;
        if (span.Contains(Slugs.Genarians.AsSpan(), StringComparison.OrdinalIgnoreCase)) return 1;
        if (span.Contains(Slugs.Imdb.AsSpan(), StringComparison.OrdinalIgnoreCase)) return 2;
        if (span.Contains(Slugs.OnThisDay.AsSpan(), StringComparison.OrdinalIgnoreCase)) return 3;

        return 4;
    }
}