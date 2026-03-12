using BirthdayCollator.Server.Configuration;
using BirthdayCollator.Server.Helpers;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Sources;

namespace BirthdayCollator.Server.Processing.Enrichment
{
    public sealed class LeapYearSourceDecorator(IBirthSource innerSource) : IBirthSource
    {
        public async Task<List<Person>> GetPeopleAsync(DateTime actualDate, CancellationToken token)
        {
            var people = await innerSource.GetPeopleAsync(actualDate, token);

            if (LeapYear.IsNonLeapFeb28(actualDate.Month, actualDate.Day))
            {
                var leapDay = actualDate.AddDays(1);
                var leapResults = await innerSource.GetPeopleAsync(leapDay, token);
                people.AddRange(leapResults);
            }

            return people;
        }
        public bool IsRelevant(BirthSourceOptions opt, IYearRangeProvider years, DateTime date) => innerSource.IsRelevant(opt, years, date);
    }
}