using BirthdayCollator.Server.Models;

namespace BirthdayCollator.Server.Processing.Parsers
{
    public interface IDatePageParser
    {
        List<Person> Parse(string html, int month, int day);
    }
}