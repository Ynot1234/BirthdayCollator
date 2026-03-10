using BirthdayCollator.Server.Infrastructure.Throttling;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Fetching;

namespace BirthdayCollator.Server.Processing.Pipelines;

public record PipelineOptions(
    IReadOnlyList<string> Years,
    IReadOnlyList<string> Suffixes,
    Func<string, string, string> SlugBuilder,
    string XPath,
    bool UseThrottle,
    Func<string, Exception, Task>? LogError,
    WikiHtmlFetcher Fetcher,
    DateTime ActualDate,
    bool IncludeAll
);

public sealed class BirthSourceEngine(IWikiParser parser)
{
    public async Task<List<Person>> RunAsync(PipelineOptions options, CancellationToken token)
    {
        var tasks = options.Years.SelectMany(year =>
            options.Suffixes.Select(suffix =>
                ProcessYearSuffixAsync(options, year, suffix, token)
            )
        );

        var results = await Task.WhenAll(tasks);
        return [.. results.SelectMany(x => x)];
    }

    private async Task<List<Person>> ProcessYearSuffixAsync(
        PipelineOptions opt, string year, string suffix, CancellationToken ct)
    {
        async Task<List<Person>> ExecuteFetchAndParse()
        {
            ct.ThrowIfCancellationRequested();

            string slug = opt.SlugBuilder(year, suffix);
            string html = await opt.Fetcher.FetchHtmlAsync(slug, ct);
          
            var contextualDate = new DateTime(
                int.Parse(year),
                opt.ActualDate.Month,
                opt.ActualDate.Day
            );

            return parser.Parse(html, contextualDate, suffix, opt.XPath, opt.IncludeAll);
        }

        try
        {
            return opt.UseThrottle
                ? await Throttle.Category.RunAsync(ExecuteFetchAndParse)
                : await ExecuteFetchAndParse();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (opt.LogError is not null)
                await opt.LogError($"{year} {suffix}".Trim(), ex);

            return [];
        }
    }
}