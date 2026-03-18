namespace BirthdayCollator.Server.Processing.Sources
{
    public sealed class GenariansBirthSource(GenariansEngine genarians) : IBirthSource
    {
        public async Task<List<Person>> GetPeopleAsync(DateTime actualDate, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return await genarians.ScrapeAllAsync(
                actualDate.ToString(DateFormats.MonthLong, InvariantCulture),
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