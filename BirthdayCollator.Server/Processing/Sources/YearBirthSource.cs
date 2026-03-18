namespace BirthdayCollator.Server.Processing.Sources;

public sealed class YearBirthSource(BirthSourceEngine engine, WikiHtmlFetcher fetcher, IYearRangeProvider yearRangeProvider) : IBirthSource
{
    public async Task<List<Person>> GetPeopleAsync(DateTime actualDate, CancellationToken token)
    {
        var years = actualDate is { Month: 2, Day: 29 } ? yearRangeProvider.GetLeapYears() : yearRangeProvider.GetYears();
        return await RunPipeline(years, actualDate, yearRangeProvider.IncludeAll, token);
    }

    private Task<List<Person>> RunPipeline(IEnumerable<string> years, DateTime date, bool includeAll, CancellationToken token) =>
        engine.RunAsync(new PipelineOptions(
            Years: [.. years],
            Suffixes: [""],
            SlugBuilder: (year, _) => year,
            XPath: XPathSelectors.YearBirthsHeader,
            UseThrottle: false,
            LogError: null,
            Fetcher: fetcher,
            ActualDate: date,
            IncludeAll: includeAll
        ), token);

    public bool IsRelevant(BirthSourceOptions opt, IYearRangeProvider years, DateTime date) => opt.EnableYearParser;
}