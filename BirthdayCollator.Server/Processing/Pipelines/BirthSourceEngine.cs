using BirthdayCollator.Server.Infrastructure.Throttling;

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

public sealed class BirthSourceEngine(IWikiParser parser, IThrottleRegistry throttles)
{
    public async Task<List<Person>> RunAsync(PipelineOptions options, CancellationToken token)
    {
        var taskCount = options.Years.Count * options.Suffixes.Count;
        if (taskCount == 0) return [];

        var tasks = new List<Task<List<Person>>>(taskCount);

        foreach (var year in options.Years)
        {
            foreach (var suffix in options.Suffixes)
            {
                tasks.Add(ProcessYearSuffixAsync(options, year, suffix, token));
            }
        }

        List<Person> allPeople = [];

        await foreach (var completedTask in Task.WhenEach(tasks))
        {
            var results = await completedTask;
            if (results.Count > 0)
            {
                allPeople.AddRange(results);
            }
        }

        return allPeople;
    }

    private async Task<List<Person>> ProcessYearSuffixAsync(PipelineOptions opt, string year, string suffix, CancellationToken ct)
    {
        async Task<List<Person>> ExecuteFetchAndParse()
        {
            ct.ThrowIfCancellationRequested();

            string slug = opt.SlugBuilder(year, suffix);
            string html = await opt.Fetcher.FetchHtmlAsync(slug, ct);

            int parsedYear = int.TryParse(year, out int y) ? y : opt.ActualDate.Year;
            DateTime contextualDate = new(parsedYear, opt.ActualDate.Month, opt.ActualDate.Day);

            return parser.Parse(html, contextualDate, suffix, opt.XPath, opt.IncludeAll);
        }

        try
        {
            return opt.UseThrottle
                ? await throttles.Category.RunAsync(ExecuteFetchAndParse)
                : await ExecuteFetchAndParse();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (opt.LogError is not null)
            {
                await opt.LogError($"{year} {suffix}".Trim(), ex);
            }
            return [];
        }
    }
}