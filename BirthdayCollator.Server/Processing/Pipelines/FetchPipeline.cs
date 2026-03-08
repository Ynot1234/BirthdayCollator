using BirthdayCollator.Server.Configuration;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Pipelines;
using BirthdayCollator.Server.Processing.Sources;
using Microsoft.Extensions.Options;

namespace BirthdayCollator.Server.Processing.Pipelines;

public sealed class FetchPipeline(
    IEnumerable<IBirthSource> sources,
    IOptions<BirthSourceOptions> options,
    IYearRangeProvider years
) : IFetchPipeline
{
    private string[]? _debugSuffixes;

    public async Task<List<Person>> FetchAllAsync(DateTime date, CancellationToken token)
    {
        var opt = options.Value;

        var tasks = sources
            .Where(s => s.IsRelevant(opt, years, date))
            .Select(async s => {
                try
                {
                    if (s is CategoryBirthSource cat && _debugSuffixes != null)
                        cat.ForceSuffixes(_debugSuffixes);

                    return await s.GetPeopleAsync(date, token);
                }
                catch { return []; }
            });

        var results = await Task.WhenAll(tasks);
        return [.. results.SelectMany(x => x)];
    }

    public void ForceSuffixes(params string[] suffixes)
    {
        _debugSuffixes = suffixes;
        if (suffixes is { Length: > 0 }) years.ForceSuffix(suffixes[0]);
    }

    public void ResetSuffixes() 
    {
        _debugSuffixes = null; years.ClearSuffix(); 
    }
}