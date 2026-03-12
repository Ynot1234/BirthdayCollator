using BirthdayCollator.Helpers;
using BirthdayCollator.Server.Models;
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
            // 1. Fast static check
            if (RegexPatterns.ExcludeDied().IsMatch(p.Description))
                return;

            // 2. Short-circuit if no URL available
            if (string.IsNullOrWhiteSpace(p.Url))
            {
                livingPeople.Add(p);
                return;
            }

            // 3. Wikipedia Verification & Description Update
            // We've moved the HTML fetching and description cleaning inside this helper
            if (!await IsLikelyDeadAsync(p, token))
            {
                livingPeople.Add(p);
            }
        });

        return [.. livingPeople.OrderByDescending(p => p.BirthYear)];
    }

    private async Task<bool> IsLikelyDeadAsync(Person p, CancellationToken ct)
    {
        try
        {
            string slug = p.Url.Split('/').Last();
            string html = await fetcher.FetchHtmlAsync(slug, ct);
            string? rawBio = WikiTextUtility.GetRawFirstParagraph(html);
            string? paren = WikiTextUtility.ExtractBioParenthetical(rawBio ?? "");

            if (WikiTextUtility.IndicatesDeath(paren, p.BirthDate))
            {
                return true; 
            }

            string? cleanDescription = WikiTextUtility.GetFirstBioParagraph(html);
            if (!string.IsNullOrWhiteSpace(cleanDescription))
            {
                p.Description = cleanDescription;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}