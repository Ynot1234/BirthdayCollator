using BirthdayCollator.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BirthdayCollator.Processing;

public interface IPersonPipeline
{
    Task<List<Person>> Process(List<Person> people);
}
