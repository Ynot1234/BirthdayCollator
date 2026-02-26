using BirthdayCollator.Server.Models;

namespace BirthdayCollator.Server.Processing.Sorting;

public class PersonSorter()
{
    public List<Person> SortPersons(List<Person> people)
    {
        List<Person> sorted =
        [
            .. people.OrderBy(p => p.BirthYear)
        ];

        return sorted;
    }
}