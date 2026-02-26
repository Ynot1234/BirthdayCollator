using BirthdayCollator.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BirthdayCollator.Server.Processing.Sources
{
    public interface IBirthSource
    {
        Task<List<Person>> GetPeopleAsync(DateTime actualDate, CancellationToken token);
    }
}