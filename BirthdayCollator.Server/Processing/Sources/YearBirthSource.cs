using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Helpers;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Fetching;
using BirthdayCollator.Server.Processing.Pipelines;

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

        // Always fetch the normal year pages
        var normal = await engine.RunAsync(
            years: yearRangeProvider.GetYears(),
            suffixes: [""],
            slugBuilder: (year, _) => year,
            xpath: XPathSelectors.YearBirthsHeader,
            useThrottle: false,
            logError: null,
            fetcher: fetcher,
            actualDate: actualDate,
            token: token);

        people.AddRange(normal);

        if (LeapYear.IsNonLeapFeb28(actualDate.Month, actualDate.Day))
        {
            var feb29 = await engine.RunAsync(
                years: yearRangeProvider.GetLeapYears(),
                suffixes: [""],
                slugBuilder: (year, _) => year,
                xpath: XPathSelectors.YearBirthsHeader,
                useThrottle: false,
                logError: null,
                fetcher: fetcher,
                actualDate: new DateTime(actualDate.Year, actualDate.Month, actualDate.Day + 1),
                token: token);

            people.AddRange(feb29);
        }

        return people;
    }

}