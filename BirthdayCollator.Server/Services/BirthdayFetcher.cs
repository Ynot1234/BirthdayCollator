using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Pipelines;
using Microsoft.Extensions.Caching.Memory;

namespace BirthdayCollator.Server.Services;

public sealed class BirthdayFetcher(
    IFetchPipeline fetchPipeline,
    IPersonPipeline personPipeline,
    IYearRangeProvider years,
    IMemoryCache cache)
{
    private static string GetCacheKey(int m, int d, bool includeAll) => $"birthdays:{m}:{d}:all:{includeAll}";

    public async Task<List<Person>> GetBirthdays(int month, int day, bool includeAll, CancellationToken token)
    {
        years.SetIncludeAll(includeAll);

        DateTime date = new(DateTime.Now.Year, month, day);

        var rawPeople = await fetchPipeline.FetchAllAsync(date, token);
        var processed = await personPipeline.Process(rawPeople, token);

        return processed;
    }

    public void ClearCache(int month, int day)
    {
        cache.Remove(GetCacheKey(month, day, true));
        cache.Remove(GetCacheKey(month, day, false));
    }

    public bool IsCached(int month, int day)
    {
        return cache.TryGetValue(GetCacheKey(month, day, true), out _) ||
               cache.TryGetValue(GetCacheKey(month, day, false), out _);
    }
}