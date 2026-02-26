using BirthdayCollator.Constants;
using BirthdayCollator.Models;
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

    public Task<List<Person>> GetPeopleAsync(DateTime actualDate, CancellationToken token)
    {
        string[] suffixes =
            _debugSuffixes ??
            config.GetSection("CategorySuffixes").Get<string[]>() ?? [];

        IReadOnlyList<string> years = yearRangeProvider.GetYears();

        return engine.RunAsync(
            years: years,
            suffixes: suffixes,
            slugBuilder: (year, suffix) => $"{year}_{suffix}",
            xpath: XPathSelectors.CategoryBirthsHeader,
            useThrottle: true,
            logError: (slug, ex) =>
            {
                logger.LogError(ex, "Failed to fetch or parse slug '{Slug}'", slug);

                string message =
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | SLUG: {slug} | ERROR: {ex.Message}{Environment.NewLine}";

                return Task.CompletedTask;
            },
            fetcher: fetcher,
            actualDate: actualDate,
            token: token);
    }
}