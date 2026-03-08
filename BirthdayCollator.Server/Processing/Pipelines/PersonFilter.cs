using BirthdayCollator.Helpers;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Fetching;
using BirthdayCollator.Server.Processing.Html;

namespace BirthdayCollator.Server.Processing.Pipelines; 
public sealed class PersonFilter(WikiHtmlFetcher fetcher)
{
    public async Task<List<Person>> FilterLivingAsync(List<Person> people, CancellationToken ct)
    {
        var livingPeople = new System.Collections.Concurrent.ConcurrentBag<Person>();

       await Parallel.ForEachAsync(people, new ParallelOptions { MaxDegreeOfParallelism = 5, CancellationToken = ct },
       async (p, token) =>
       {
           if (RegexPatterns.ExcludeDiedRegex().IsMatch(p.Description))
               return;

           if (string.IsNullOrWhiteSpace(p.Url))
           {
               livingPeople.Add(p);
               return;
           }

           if (!await IsLikelyDeadAsync(p, token))
           {
               livingPeople.Add(p);
           }
       });



        return [.. livingPeople.OrderByDescending(p => p.BirthYear)];
    }

    private async Task<bool> IsLikelyDeadAsync(Person p, CancellationToken ct)
    {
        if (RegexPatterns.ExcludeDiedRegex().IsMatch(p.Description))
            return true;

        if (p.Url is null ||
            !p.Url.Contains("wikipedia.org", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string slug = WikiUrlBuilder.ExtractSlug(p.Url);
        string html = await fetcher.FetchHtmlAsync(slug, ct);
        string? bioText = WikiTextUtility.GetFirstBioParagraph(html);
        string? paren = WikiTextUtility.ExtractBioParenthetical(bioText ?? "");

        if (string.IsNullOrEmpty(paren))
            return false;

        if (paren.Contains('–') &&
            !paren.Contains("present", StringComparison.OrdinalIgnoreCase))
            return true;

        return IsDateMismatch(paren, p.BirthDate);
    }


    private static bool IsDateMismatch(string text, DateTime birthDate)
    {
        var match = RegexPatterns.LongFormDateRegex().Match(text);
        return match.Success && DateTime.TryParse(match.Value, out var parsed)
               && (parsed.Month != birthDate.Month || parsed.Day != birthDate.Day);
    }
}
