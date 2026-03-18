namespace BirthdayCollator.Server.Processing.Cleaning;

public sealed class PersonCleaner
{
    public List<Person> CleanPersons(List<Person> people)
    {
        if (people.Count == 0) return [];

        List<Person> cleanedList = new(people.Count);

        foreach (var p in people)
        {
            string cleanName = WikiTextUtility.SanitizeWikiText(p.Name);
            string cleanDesc = WikiTextUtility.SanitizeWikiText(p.Description);

            if (string.IsNullOrWhiteSpace(cleanDesc)) continue;

            if (!string.IsNullOrEmpty(cleanName))
            {
                cleanDesc = cleanDesc
                    .Replace(cleanName, String.Empty, StringComparison.OrdinalIgnoreCase)
                    .TrimDebris();
            }

            var cleaned = p.Clone();
            cleaned.Name = cleanName;
            cleaned.Description = cleanDesc;
            cleanedList.Add(cleaned);
        }
        return cleanedList;
    }
}