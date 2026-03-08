using BirthdayCollator.Server.Configuration;
using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Helpers;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Fetching;
using BirthdayCollator.Server.Processing.Pipelines;
using static BirthdayCollator.Server.Processing.Pipelines.BirthSourceEngine;

namespace BirthdayCollator.Server.Processing.Sources;

public sealed class YearBirthSource(
    BirthSourceEngine engine,
    WikiHtmlFetcher fetcher,
    IYearRangeProvider yearRangeProvider
) : IBirthSource
{
    public async Task<List<Person>> GetPeopleAsync(DateTime actualDate, CancellationToken token)
    {
        List<Person> people = [];
        bool includeAll = yearRangeProvider.IncludeAll;

        var normal = await engine.RunAsync(new PipelineOptions(
        Years: yearRangeProvider.GetYears(),
        Suffixes: [""],
        SlugBuilder: (year, _) => year,
        XPath: XPathSelectors.YearBirthsHeader,
        UseThrottle: false,
        LogError: null,
        Fetcher: fetcher,
        ActualDate: actualDate,
        IncludeAll: includeAll
    ), token);


        people.AddRange(normal);

        if (LeapYear.IsNonLeapFeb28(actualDate.Month, actualDate.Day))
        {
            var feb29 = 
                await engine.RunAsync(new PipelineOptions(
                                    Years: yearRangeProvider.GetLeapYears(),
                                    Suffixes: [""],
                                    SlugBuilder: (year, _) => year,
                                    XPath: XPathSelectors.YearBirthsHeader,
                                    UseThrottle: false,
                                    LogError: null,
                                    Fetcher: fetcher,
                                    ActualDate: new DateTime(actualDate.Year, actualDate.Month, actualDate.Day + 1),
                                    IncludeAll: includeAll
                                ), token);


            people.AddRange(feb29);
        }

        return people;
    }

    public bool IsRelevant(BirthSourceOptions opt, IYearRangeProvider years, DateTime date) => opt.EnableYearParser;
}