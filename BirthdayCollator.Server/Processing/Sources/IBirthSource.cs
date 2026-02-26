using BirthdayCollator.Server.Models;

namespace BirthdayCollator.Server.Processing.Sources;

public interface IBirthSource
{
    Task<List<Person>> GetPeopleAsync(DateTime actualDate, CancellationToken token);
}

