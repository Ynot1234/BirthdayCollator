using BirthdayCollator.Server.Models;

namespace BirthdayCollator.Server.Processing.Pipelines;
public interface IWikiParser
{
    List<Person> Parse(
     string html,
     DateTime birthDate,
     string? suffix,
     string xpath,
     bool includeAll);
}