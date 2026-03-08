using BirthdayCollator.Server.Configuration;
using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Helpers;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Fetching;
using BirthdayCollator.Server.Processing.Pipelines;
using BirthdayCollator.Server.Processing.Sources;
using static BirthdayCollator.Server.Processing.Pipelines.BirthSourceEngine;

public sealed class CategoryBirthSource(
    BirthSourceEngine engine,
    WikiHtmlFetcher fetcher,
    IYearRangeProvider yearRangeProvider,
    IConfiguration config,
    ILogger<CategoryBirthSource> logger
) : IBirthSource
{
    private string[]? _debugSuffixes;

    public void ForceSuffixes(params string[] suffixes) => _debugSuffixes = suffixes;
    public void ResetSuffixes() => _debugSuffixes = null;

    public async Task<List<Person>> GetPeopleAsync(DateTime actualDate, CancellationToken token)
    {
        string[] suffixes = _debugSuffixes ?? config.GetSection("CategorySuffixes").Get<string[]>() ?? [];
        bool includeAll = yearRangeProvider.IncludeAll;
        var people = await RunPipeline(yearRangeProvider.GetYears(), actualDate, suffixes, includeAll, token);

        if (LeapYear.IsNonLeapFeb28(actualDate.Month, actualDate.Day))
        {
            var leapDay = new DateTime(actualDate.Year, 2, 29);
            var feb29 = await RunPipeline(yearRangeProvider.GetLeapYears(), leapDay, suffixes, includeAll, token);
            people.AddRange(feb29);
        }

        return people;
    }

    private Task<List<Person>> RunPipeline(IEnumerable<string> years, DateTime date, string[] suffixes, bool includeAll, CancellationToken token)
    {
        return engine.RunAsync(new PipelineOptions(
            Years: [.. years],
            Suffixes: suffixes,
            SlugBuilder: (year, suffix) => $"{year}_{suffix}",
            XPath: XPathSelectors.CategoryBirthsHeader,
            UseThrottle: true,
            LogError: (slug, ex) => {
                logger.LogError(ex, "Failed to fetch or parse slug '{Slug}'", slug);
                return Task.CompletedTask;
            },
            Fetcher: fetcher,
            ActualDate: date,
            IncludeAll: includeAll
        ), token);
    }

    public bool IsRelevant(BirthSourceOptions opt, IYearRangeProvider years, DateTime date) => opt.EnableCategoryParser;
}