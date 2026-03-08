using BirthdayCollator.Helpers;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Fetching;
using BirthdayCollator.Server.Processing.Html;

namespace BirthdayCollator.Server.Processing.Pipelines;

public sealed class PersonFilter(WikiHtmlFetcher fetcher)
{
    public async Task<List<Person>> FilterLivingAsync(List<Person> people, CancellationToken ct)
    {
        var results = new bool[people.Count];

        await Parallel.ForEachAsync(people.Select((p, i) => (p, i)),
            new ParallelOptions { MaxDegreeOfParallelism = 5, CancellationToken = ct },
            async (item, token) =>
            {
                results[item.i] = await IsLikelyDeadAsync(item.p, token);
            });

        return [.. people.Where((_, i) => !results[i])];
    }

    private async Task<bool> IsLikelyDeadAsync(Person p, CancellationToken ct)
    {
        if (p.Description.Contains("died", StringComparison.OrdinalIgnoreCase))
            return true;

        try
        {
            string html = await fetcher.FetchHtmlAsync(p.Url, ct);
            string? bioText = WikipediaDomNavigator.GetFirstBioParagraph(html);

            if (string.IsNullOrWhiteSpace(bioText))
                return false;

            string? paren = WikiTextUtility.ExtractBioParenthetical(bioText);

            if (string.IsNullOrEmpty(paren))
                return false;

            if (paren.Contains('–'))
                return true;

            return IsDateMismatch(paren, p.BirthDate);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsDateMismatch(string text, DateTime birthDate)
    {
        var match = RegexPatterns.LongFormDateRegex().Match(text);
        if (!match.Success || !DateTime.TryParse(match.Value, out var parsed))
            return false;

        return parsed.Month != birthDate.Month || parsed.Day != birthDate.Day;
    }
}
