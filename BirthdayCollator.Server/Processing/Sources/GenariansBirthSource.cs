using BirthdayCollator.Models;
using BirthdayCollator.Resources;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
