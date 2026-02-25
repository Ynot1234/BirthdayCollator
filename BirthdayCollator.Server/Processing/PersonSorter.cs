using BirthdayCollator.Models;
using System.Collections.Generic;
using System.Linq;

namespace BirthdayCollator.Processing
{
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
}
