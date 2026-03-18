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
            .Select(s =>
            {
                if (s is CategoryBirthSource cat && _debugSuffixes != null)
                {
                    cat.ForceSuffixes(_debugSuffixes);
                }
                return s.GetPeopleAsync(date, token);
            })
            .ToList();

        if (tasks.Count == 0) return [];

        List<Person> allPeople = [];

        await foreach (var sourceTask in Task.WhenEach(tasks))
        {
               var sourceResults = await sourceTask;
                if (sourceResults.Count > 0)
                {
                    allPeople.AddRange(sourceResults);
                }
         
        }

        return allPeople;
    }

    public void ForceSuffixes(params string[] suffixes)
    {
        _debugSuffixes = suffixes;
        if (suffixes is { Length: > 0 })
        {
            years.ForceSuffix(suffixes[0]);
        }
    }

    public void ResetSuffixes()
    {
        _debugSuffixes = null;
        years.ClearSuffix();
    }
}