namespace BirthdayCollator.Server.Processing.Sources;

public interface IBirthSource
{
    Task<List<Person>> GetPeopleAsync(DateTime actualDate, CancellationToken token);

    bool IsRelevant(BirthSourceOptions opt, IYearRangeProvider years, DateTime date) => true;
}