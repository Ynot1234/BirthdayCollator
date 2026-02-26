using BirthdayCollator.Server.Models;

namespace BirthdayCollator.Server.Processing.Pipelines;

public interface IPersonPipeline
{
    Task<List<Person>> Process(List<Person> people);
}