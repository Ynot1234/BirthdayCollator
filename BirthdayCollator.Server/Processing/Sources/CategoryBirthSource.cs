using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Helpers;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Fetching;
using BirthdayCollator.Server.Processing.Pipelines;
using static BirthdayCollator.Server.Processing.Pipelines.BirthSourceEngine;

namespace BirthdayCollator.Server.Processing.Sources;

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

    public async Task<List<Person>> GetPeopleAsync(
        DateTime actualDate,
        CancellationToken token)
    {
        string[] suffixes =
            _debugSuffixes ??
            config.GetSection("CategorySuffixes").Get<string[]>() ?? [];

        List<Person> people = [];

        bool includeAll = yearRangeProvider.IncludeAll;

        var normal = await engine.RunAsync(new PipelineOptions(
                      Years: yearRangeProvider.GetYears(),
                      Suffixes: suffixes,
                      SlugBuilder: (year, suffix) => $"{year}_{suffix}",
                      XPath: XPathSelectors.CategoryBirthsHeader,
                      UseThrottle: true,
                      LogError: (slug, ex) =>
                      {
                          logger.LogError(ex, "Failed to fetch or parse slug '{Slug}'", slug);
                          return Task.CompletedTask;
                      },
                      Fetcher: fetcher,
                      ActualDate: actualDate,
                      IncludeAll: includeAll
                  ), token);


        people.AddRange(normal);

        if (LeapYear.IsNonLeapFeb28(actualDate.Month, actualDate.Day))
        {
              var feb29 = await engine.RunAsync(new PipelineOptions(
                                            Years: yearRangeProvider.GetLeapYears(),
                                            Suffixes: suffixes,
                                            SlugBuilder: (year, suffix) => $"{year}_{suffix}",
                                            XPath: XPathSelectors.CategoryBirthsHeader,
                                            UseThrottle: true,
                                            LogError: (slug, ex) =>
                                            {
                                                logger.LogError(ex, "Failed to fetch or parse slug '{Slug}'", slug);
                                                return Task.CompletedTask;
                                            },
                                            Fetcher: fetcher,
                                            ActualDate: new DateTime(actualDate.Year, actualDate.Month, actualDate.Day + 1),
                                            IncludeAll: includeAll), token);
              people.AddRange(feb29);
        }

        return people;
    }
}