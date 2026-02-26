using BirthdayCollator.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BirthdayCollator.Server.Processing.Pipelines;

public interface IPersonPipeline
{
    Task<List<Person>> Process(List<Person> people);
}
