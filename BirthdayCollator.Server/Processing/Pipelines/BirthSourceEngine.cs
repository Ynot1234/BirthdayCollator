using BirthdayCollator.Helpers;
using BirthdayCollator.Models;
using BirthdayCollator.Server.Infrastructure.Throttling;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Dates;
using BirthdayCollator.Server.Processing.Entries;
using BirthdayCollator.Server.Processing.Fetching;
using BirthdayCollator.Server.Processing.Html;
using BirthdayCollator.Server.Processing.Links;
using BirthdayCollator.Server.Processing.Names;
using BirthdayCollator.Server.Processing.Parsers;
using BirthdayCollator.Server.Processing.Validation;

namespace BirthdayCollator.Server.Processing.Pipelines;

public sealed class BirthSourceEngine(
    IHtmlBirthSectionExtractor htmlExtractor,
    IBirthDateParser dateParser,
    IEntrySplitter entrySplitter,
    ILinkResolver linkResolver,
    IPersonNameResolver nameResolver
)
{
    public async Task<List<Person>> RunAsync(
        IReadOnlyList<string> years,
        IReadOnlyList<string> suffixes,
        Func<string, string, string> slugBuilder,
        string xpath,
        bool useThrottle,
        Func<string, Exception, Task>? logError,
        WikiHtmlFetcher fetcher,
        DateTime actualDate,
        CancellationToken token)
    {
        BirthEntryValidator validator = new([.. years], RegexPatterns.ExcludeDiedRegex());
        PersonFactory factory = new(WikiUrlBuilder.NormalizeWikiHref, nameResolver);

        Parser parser = new(
            validator,
            factory,
            htmlExtractor,
            dateParser,
            entrySplitter,
            linkResolver,
            nameResolver
        );

        List<Task<List<Person>>> tasks = [];

        foreach (string year in years)
        {
            token.ThrowIfCancellationRequested();

            DateTime adjustedDate = new(int.Parse(year), actualDate.Month, actualDate.Day);

            foreach (string suffix in suffixes)
            {
                token.ThrowIfCancellationRequested();

                string slug = slugBuilder(year, suffix);

                tasks.Add(ProcessAsync(
                    slug,
                    adjustedDate,
                    suffix,
                    xpath,
                    useThrottle,
                    logError,
                    fetcher,
                    parser,
                    token));
            }
        }

        List<Person>[] results = await Task.WhenAll(tasks);
        return [.. results.SelectMany(x => x)];
    }

    private static async Task<List<Person>> ProcessAsync(
        string slug,
        DateTime adjustedDate,
        string? suffix,
        string xpath,
        bool useThrottle,
        Func<string, Exception, Task>? logError,
        WikiHtmlFetcher fetcher,
        Parser parser,
        CancellationToken token)
    {
        if (useThrottle)
            await Throttle.Category.WaitAsync();

        try
        {
            token.ThrowIfCancellationRequested();
            string html = await fetcher.FetchHtmlAsync(slug, token);
            token.ThrowIfCancellationRequested();
            return parser.Parse(html, adjustedDate, suffix, xpath);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            if (logError is not null)
                await logError(slug, ex);

            return [];
        }
        finally
        {
            if (useThrottle)
                Throttle.Category.Release();
        }
    }
}
