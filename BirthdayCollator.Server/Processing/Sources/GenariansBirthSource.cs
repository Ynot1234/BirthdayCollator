using BirthdayCollator.Server.Configuration;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Resources;
using System.Globalization;
using static BirthdayCollator.Server.Constants.AppStrings;

namespace BirthdayCollator.Server.Processing.Sources
{
    public sealed class GenariansBirthSource(Genarians genarians) : IBirthSource
    {
        public async Task<List<Person>> GetPeopleAsync(DateTime actualDate, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return await genarians.ScrapeAllAsync(
                actualDate.ToString(DateFormats.MonthLong, CultureInfo.InvariantCulture),
                actualDate.Day,
                token);
        }
        public bool IsRelevant(BirthSourceOptions opt, IYearRangeProvider years, DateTime date)
        {
            if (!opt.EnableGenarianParser) return false;

            int cutoff = date.Year - 90;
            return years.GetYears().Any(y => int.TryParse(y, out int yr) && yr <= cutoff);
        }
    }
}