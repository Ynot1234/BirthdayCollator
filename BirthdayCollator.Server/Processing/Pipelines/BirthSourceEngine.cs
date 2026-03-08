using BirthdayCollator.Server.Infrastructure.Throttling;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Fetching;

namespace BirthdayCollator.Server.Processing.Pipelines;

public sealed class BirthSourceEngine(IWikiParser parser) 
{

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


    public async Task<List<Person>> RunAsync(
        PipelineOptions options, // Group those 10 params into a record
        CancellationToken token)
    {
        var tasks = options.Years.SelectMany(year =>
            options.Suffixes.Select(suffix => {
                var date = new DateTime(int.Parse(year), options.ActualDate.Month, options.ActualDate.Day);
                return ProcessAsync(options, year, suffix, date, token);
            })
        );

        var results = await Task.WhenAll(tasks);
        return [.. results.SelectMany(x => x)];
    }

    private async Task<List<Person>> ProcessAsync(
        PipelineOptions opt, string year, string suffix, DateTime date, CancellationToken ct)
    {
        async Task<List<Person>> Execute()
        {
            ct.ThrowIfCancellationRequested();
            var slug = opt.SlugBuilder(year, suffix);
            var html = await opt.Fetcher.FetchHtmlAsync(slug, ct);
            return parser.Parse(html, date, suffix, opt.XPath, opt.IncludeAll);
        }

        try
        {
            return opt.UseThrottle
                ? await Throttle.Category.RunAsync(Execute)
                : await Execute();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (opt.LogError is not null)
                await opt.LogError($"{year} {suffix}".Trim(), ex);
            return [];
        }
    }

}