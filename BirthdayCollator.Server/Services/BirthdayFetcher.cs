using BirthdayCollator.Models;
using BirthdayCollator.Processing;
using BirthdayCollator.Server.Processing;

namespace BirthdayCollator.Server.Services;

public sealed class BirthdayFetcher(
    IFetchPipeline fetchPipeline,
    IPersonPipeline personPipeline)
{
    public async Task<List<Person>> GetBirthdays(int month, int day, CancellationToken token)
    {
        DateTime date = new(DateTime.Now.Year, month, day);

        // Pass the token into every async operation
        var rawPeople = await fetchPipeline.FetchAllAsync(date, token);
        var processed = await personPipeline.Process(rawPeople);

        return processed;
    }

}
