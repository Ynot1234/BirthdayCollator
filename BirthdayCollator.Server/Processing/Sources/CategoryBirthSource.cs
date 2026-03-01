using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Helpers;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Fetching;
using BirthdayCollator.Server.Processing.Pipelines;

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

        // Always fetch the normal year pages
        var normal = await engine.RunAsync(
            years: yearRangeProvider.GetYears(),
            suffixes: suffixes,
            slugBuilder: (year, suffix) => $"{year}_{suffix}",
            xpath: XPathSelectors.CategoryBirthsHeader,
            useThrottle: true,
            logError: (slug, ex) =>
            {
                logger.LogError(ex, "Failed to fetch or parse slug '{Slug}'", slug);
                return Task.CompletedTask;
            },
            fetcher: fetcher,
            actualDate: actualDate,
            token: token);

        people.AddRange(normal);

        if (LeapYear.IsNonLeapFeb28(actualDate.Month, actualDate.Day))
        {
            var feb29 = await engine.RunAsync(
                years: yearRangeProvider.GetLeapYears(),
                suffixes: suffixes,
                slugBuilder: (year, suffix) => $"{year}_{suffix}",
                xpath: XPathSelectors.CategoryBirthsHeader,
                useThrottle: true,
                logError: (slug, ex) =>
                {
                    logger.LogError(ex, "Failed to fetch or parse slug '{Slug}'", slug);
                    return Task.CompletedTask;
                },
                fetcher: fetcher,
                actualDate: new DateTime(actualDate.Year, actualDate.Month, actualDate.Day + 1),
                token: token);

            people.AddRange(feb29);
        }

        return people;
    }


}