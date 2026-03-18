namespace BirthdayCollator.Server.Processing.Sorting;

public sealed class PersonSorter(IYearRangeProvider yearRangeProvider)
{
    public List<Person> SortPersons(List<Person> people)
    {
        if (yearRangeProvider.IncludeAll)
        {
            return [.. people
                .OrderBy(p => p.Month)
                .ThenBy(p => p.Day)
                .ThenBy(p => p.Name)];
        }

        return [.. people
            .OrderBy(p => p.BirthYear)
            .ThenBy(p => p.Name)];
    }
}