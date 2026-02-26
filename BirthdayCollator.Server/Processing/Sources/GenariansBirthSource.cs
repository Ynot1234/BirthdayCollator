using BirthdayCollator.Resources;
using BirthdayCollator.Server.Models;

namespace BirthdayCollator.Server.Processing.Sources
{
    public sealed class GenariansBirthSource(Genarians genarians) : IBirthSource
    {
        public async Task<List<Person>> GetPeopleAsync(DateTime actualDate, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return await genarians.ScrapeAllGenariansAsync(actualDate.ToString("MMMM"), actualDate.Day, token);
        }
    }
}
