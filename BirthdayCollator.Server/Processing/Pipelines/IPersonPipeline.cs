namespace BirthdayCollator.Server.Processing.Pipelines;
public interface IPersonPipeline
{
    Task<List<Person>> Process(List<Person> people, CancellationToken token);
}