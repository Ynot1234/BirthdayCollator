using BirthdayCollator.Server.Configuration;
using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Helpers;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Fetching;
using BirthdayCollator.Server.Processing.Pipelines;
using BirthdayCollator.Server.Processing.Sources;
using static BirthdayCollator.Server.Processing.Pipelines.BirthSourceEngine;

public sealed class YearBirthSource(
    BirthSourceEngine engine,
    WikiHtmlFetcher fetcher,
    IYearRangeProvider yearRangeProvider
) : IBirthSource
{
    public async Task<List<Person>> GetPeopleAsync(DateTime actualDate, CancellationToken token)
    {
        bool includeAll = yearRangeProvider.IncludeAll;

        var people = await RunPipeline(yearRangeProvider.GetYears(), actualDate, includeAll, token);

        if (LeapYear.IsNonLeapFeb28(actualDate.Month, actualDate.Day))
        {
            var leapDay = actualDate.AddDays(1);
            var feb29 = await RunPipeline(yearRangeProvider.GetLeapYears(), leapDay, includeAll, token);
            people.AddRange(feb29);
        }

        return people;
    }

    private Task<List<Person>> RunPipeline(IEnumerable<string> years, DateTime date, bool includeAll, CancellationToken token)
    {
        return engine.RunAsync(new PipelineOptions(
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
    }

    public bool IsRelevant(BirthSourceOptions opt, IYearRangeProvider years, DateTime date) => opt.EnableYearParser;
}