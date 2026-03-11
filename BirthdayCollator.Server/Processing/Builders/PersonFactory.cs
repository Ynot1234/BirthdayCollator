using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Names;

namespace BirthdayCollator.Server.Processing.Builders;
public class PersonFactory(IPersonNameResolver nameResolver)
{
   
    public Person CreatePerson(string name, string? desc, DateTime birthDate, string url, 
                               string? sourceSlug, string section, string? displaySlug = null) =>
        Finalize(new Person
        {
            Name = name,
            Description = desc?.Trim() ?? string.Empty,
            Url = url,
            BirthYear = birthDate.Year,
            Month = birthDate.Month,
            Day = birthDate.Day,
            Section = section,
            SourceSlug = sourceSlug,
            DisplaySlug = displaySlug ?? string.Empty,
            SourceUrl = Urls.GetSourceUrl(sourceSlug, url)
        });

    public Person Finalize(Person person)
    {
        nameResolver.FixSwappedName(person);
        return person;
    }
}