using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace BirthdayCollator.Server.Processing.Pipelines;

public sealed partial class PersonFilter(WikiHtmlFetcher fetcher)
{
    public async Task<List<Person>> FilterLivingAsync(List<Person> people, CancellationToken ct)
    {
        ConcurrentBag<Person> livingPeople = [];

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = 5,
            CancellationToken = ct
        };

        await Parallel.ForEachAsync(people, options, async (p, token) =>
        {
            if (RegexPatterns.ExcludeDied().IsMatch(p.Description)) return;

            if (p.Url.Contains("imdb", StringComparison.OrdinalIgnoreCase))
            {
                livingPeople.Add(p);
                return;
            }


            if (await IsLikelyDeadAsync(p, token))
            {
                return;
            }


            bool hasWiki = p.Url.Contains("wikipedia.org", StringComparison.OrdinalIgnoreCase);

            if (!hasWiki && (string.IsNullOrWhiteSpace(p.Url) || p.SourceSlug == Slugs.OnThisDay))
            {

              

                livingPeople.Add(p);
                return;
            }

            if (!p.Url.Contains("duckduckgo", StringComparison.OrdinalIgnoreCase))
            {
                livingPeople.Add(p);
            }

          


        });

        return [.. livingPeople.OrderByDescending(p => p.BirthYear)];
    }

    private async Task<bool> IsLikelyDeadAsync(Person p, CancellationToken ct)
    {
        string slug;

        if (!string.IsNullOrWhiteSpace(p.Url) &&
            p.Url.Contains("wikipedia.org", StringComparison.OrdinalIgnoreCase))
        {
            ReadOnlySpan<char> urlSpan = p.Url.AsSpan();
            int lastSlash = urlSpan.LastIndexOf('/');

            slug = lastSlash == -1
                ? p.Url
                : new string(urlSpan[(lastSlash + 1)..]);
        }
        {
            var parts = p.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string first = parts.Length > 0 ? parts[0] : "";
            string last = parts.Length > 1 ? parts[^1] : "";
            string raw = $"{first}_{last}";
            slug = new string(raw.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        }

        string html = await fetcher.FetchHtmlAsync(slug, ct);

        string? rawBio = WikiTextUtility.GetRawFirstParagraph(html);
        string? paren = WikiTextUtility.ExtractSpecificParenthetical(rawBio ?? string.Empty, p.Name);

        if (paren is not null && RegexPatterns.YearIndicator().IsMatch(paren))
        {
            if (!IsBirthDateMatch(paren, p.BirthDate))
            {
                return true;
            }
        }

        if (WikiTextUtility.IndicatesDeath(paren, p.BirthDate))
        {
            return true;
        }

        if (WikiTextUtility.GetFirstBioParagraph(html) is { } cleanDescription)
        {
            if (string.IsNullOrWhiteSpace(p.Description))
                p.Description = cleanDescription;
        }

        return false;
    }


    public static bool IsBirthDateMatch(string? paren, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(paren)) return true;

        string year = date.Year.ToString();
        if (RegexPatterns.YearIndicator().IsMatch(paren) && !paren.Contains(year))
        {
            return false;
        }

        string month = date.ToString(DateFormats.MonthLong);
        string day = date.Day.ToString();

        bool hasAnyMonth = RegexPatterns.MonthName().IsMatch(paren);

        bool monthMatch = paren.Contains(month, StringComparison.OrdinalIgnoreCase);

        bool dayMatch = Regex.IsMatch(paren, $@"\b{day}\b");

        if (hasAnyMonth && (!monthMatch || !dayMatch))
        {
            return false;
        }

        return true;
    }
}