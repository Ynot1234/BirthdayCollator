using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BirthdayCollator.Models;
using BirthdayCollator.Processing;
using BirthdayCollator.Server.Processing;

namespace BirthdayCollator.Services;

public sealed class BirthdayFetcher(
    IFetchPipeline fetchPipeline,
    IPersonPipeline personPipeline)
{
    public async Task<List<Person>> GetBirthdays(int month, int day)
    {
        DateTime date = new(DateTime.Now.Year, month, day);
        var rawPeople = await fetchPipeline.FetchAllAsync(date, CancellationToken.None);
        var processed = await personPipeline.Process(rawPeople);
        return processed;
    }
}
