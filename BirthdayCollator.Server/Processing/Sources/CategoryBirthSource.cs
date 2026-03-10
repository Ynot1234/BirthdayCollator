using BirthdayCollator.Server.Configuration;
using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Fetching;
using BirthdayCollator.Server.Processing.Pipelines;

namespace BirthdayCollator.Server.Processing.Sources;
public sealed class CategoryBirthSource( BirthSourceEngine engine, WikiHtmlFetcher fetcher, 
                                         IYearRangeProvider yearRangeProvider, IConfiguration config) : IBirthSource
{
    private string[]? _debugSuffixes;
    public void ForceSuffixes(params string[] suffixes) => _debugSuffixes = suffixes;

    public async Task<List<Person>> GetPeopleAsync(DateTime actualDate, CancellationToken token)
    {
        var suffixes = _debugSuffixes ?? config.GetSection("CategorySuffixes").Get<string[]>() ?? [];
        var years = actualDate is { Month: 2, Day: 29 } ? yearRangeProvider.GetLeapYears() : yearRangeProvider.GetYears();
        return await RunPipeline(years, actualDate, suffixes, yearRangeProvider.IncludeAll, token);
    }

    private Task<List<Person>> RunPipeline(IEnumerable<string> years, DateTime date, string[] suffixes, bool includeAll, CancellationToken token)
    {
        var options = new PipelineOptions(
            Years: [.. years],
            Suffixes: suffixes,
            SlugBuilder: (year, suffix) => $"{year}_{suffix}",
            XPath: XPathSelectors.CategoryBirthsHeader,
            UseThrottle: true,
            LogError: null,
            Fetcher: fetcher,
            ActualDate: date,
            IncludeAll: includeAll
        );

        return engine.RunAsync(options, token);
    }

    public bool IsRelevant(BirthSourceOptions opt, IYearRangeProvider years, DateTime date) => opt.EnableCategoryParser;
}