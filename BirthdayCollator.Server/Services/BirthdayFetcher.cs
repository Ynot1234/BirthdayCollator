namespace BirthdayCollator.Server.Services;

public sealed class BirthdayFetcher(
    IFetchPipeline fetchPipeline,
    IPersonPipeline personPipeline,
    IYearRangeProvider years)
{
    public async Task<List<Person>> GetBirthdays(int month, int day, bool includeAll, CancellationToken token)
    {
        years.SetIncludeAll(includeAll);
        DateTime date = new(2000, month, day);

        var rawPeople = await fetchPipeline.FetchAllAsync(date, token);
        return await personPipeline.Process(rawPeople, token);
    }

    public void ClearCache(int month, int day) {}

    public bool IsCached(int month, int day) => false; 
}