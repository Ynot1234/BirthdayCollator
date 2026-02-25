using BirthdayCollator.Models;
using BirthdayCollator.Processing;
using Microsoft.Extensions.Options;

namespace BirthdayCollator.Server.Processing;

public sealed class FetchPipeline(
    IEnumerable<IBirthSource> sources,
    IOptions<BirthSourceOptions> options,
    IYearRangeProvider yearRangeProvider
) : IFetchPipeline
{
    private readonly BirthSourceOptions _options = options.Value;
    private readonly IYearRangeProvider _yearRangeProvider = yearRangeProvider;

    public async Task<List<Person>> FetchAllAsync(DateTime date, CancellationToken token)
    {
        DisableGenariansIfIrrelevant(date);
        List<Task<List<Person>>> tasks = [];

        foreach (IBirthSource source in sources)
        {
            if (source is YearBirthSource && !_options.EnableYearParser)
                continue;

            if (source is DateBirthSource && !_options.EnableDateParser)
                continue;

            if (source is CategoryBirthSource && !_options.EnableCategoryParser)
                continue;

            if (source is GenariansBirthSource && !_options.EnableGenarianParser)
                continue;

            if (source is OnThisDaySource && !_options.EnableOnThisDayParser)
                continue;


            if (source is CategoryBirthSource cat && _debugSuffixes is not null)
            {
                cat.ForceSuffixes(_debugSuffixes);
            }


            token.ThrowIfCancellationRequested();
            tasks.Add(RunSourceSafeAsync(source, date, token));
        }

        try
        {
            List<Person>[] results = await Task.WhenAll(tasks);
            return [.. results.SelectMany(x => x)];
        }
        catch (OperationCanceledException)
        {
            throw;
        }
    }

    private static async Task<List<Person>> RunSourceSafeAsync(
        IBirthSource source,
        DateTime date,
        CancellationToken token)
    {
        try
        {
            return await source.GetPeopleAsync(date, token);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return [];
        }
    }

    private void DisableGenariansIfIrrelevant(DateTime date)
    {
        int cutoffYear = date.Year - 90;
        IReadOnlyList<string> years = _yearRangeProvider.GetYears();

        if (!years.Any(y => int.TryParse(y, out int yr) && yr <= cutoffYear))
            _options.EnableGenarianParser = false;
    }

    private string[]? _debugSuffixes;

    public void ForceSuffixes(params string[] suffixes)
    {
        _debugSuffixes = suffixes;

        if (suffixes is { Length: > 0 })
            _yearRangeProvider.ForceSuffix(suffixes[0]);
    }

    public void ResetSuffixes()
    {
        _debugSuffixes = null;
      _yearRangeProvider.ClearSuffix();
    }
}
