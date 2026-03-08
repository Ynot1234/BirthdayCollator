using BirthdayCollator.Server.Configuration;
using BirthdayCollator.Server.Helpers;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Fetching;
using BirthdayCollator.Server.Processing.Parsers;
using BirthdayCollator.Server.Processing.Sources;

public sealed class DateBirthSource(
    WikiHtmlFetcher fetcher,
    IYearRangeProvider years,
    IDatePageParser parser 
) : IBirthSource
{
    public async Task<List<Person>> GetPeopleAsync(DateTime date, CancellationToken ct)
    {
        var people = await FetchAndParse($"{date:MMMM_d}", date.Month, date.Day, ct);

        if (date.Month == 2 && date.Day == 28 && !DateTime.IsLeapYear(date.Year))
        {
            var feb29People = await FetchAndParse("February_29", 2, 29, ct);
            var leapYears = years.GetLeapYears().ToHashSet();
            people.AddRange(feb29People.Where(p => leapYears.Contains(p.BirthYear.ToString())));
        }

        return people;
    }


    private async Task<List<Person>> FetchAndParse(string page, int m, int d, CancellationToken ct)
    {
        try
        {
            string html = await fetcher.FetchHtmlAsync(page, ct);
            return parser.Parse(html, m, d);
        }
        catch { return []; }
    }

    public bool IsRelevant(BirthSourceOptions opt, IYearRangeProvider y, DateTime d) => opt.EnableDateParser;
}