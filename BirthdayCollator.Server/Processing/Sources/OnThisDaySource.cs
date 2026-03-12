using BirthdayCollator.Server.Configuration;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Fetching;
using BirthdayCollator.Server.Processing.Parsers;

namespace BirthdayCollator.Server.Processing.Sources;

public sealed class OnThisDaySource(OnThisDayHtmlFetcher fetcher, OnThisDayParser parser, IYearRangeProvider yearRangeProvider) : IBirthSource
{
    public async Task<List<Person>> GetPeopleAsync(DateTime actualDate, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        string html = await fetcher.FetchAsync(actualDate.Month, actualDate.Day, token);
        if (string.IsNullOrWhiteSpace(html)) return [];
        var people = parser.Parse(html, actualDate.Month, actualDate.Day);
        var allowedYears = yearRangeProvider.GetYears().ToHashSet();
        return [.. people.Where(p => allowedYears.Contains(p.BirthYear.ToString()))];
    }

    public bool IsRelevant(BirthSourceOptions opt, IYearRangeProvider years, DateTime date)  => opt.EnableOnThisDayParser;
}