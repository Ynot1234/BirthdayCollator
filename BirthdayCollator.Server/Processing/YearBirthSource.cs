using BirthdayCollator.Constants;
using BirthdayCollator.Models;
using BirthdayCollator.Server.Processing;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BirthdayCollator.Processing;

public sealed class YearBirthSource(
    BirthSourceEngine engine,
    WikiHtmlFetcher fetcher,
    IYearRangeProvider yearRangeProvider
) : IBirthSource
{
    public Task<List<Person>> GetPeopleAsync(DateTime actualDate, CancellationToken token)
    {
        return engine.RunAsync(
            years: yearRangeProvider.GetYears(),
            suffixes: [""], 
            slugBuilder: (year, _) => year,
            xpath: XPathSelectors.YearBirthsHeader,
            useThrottle: false,
            logError: null,
            fetcher: fetcher,
            actualDate: actualDate,
            token: token);
    }
}
