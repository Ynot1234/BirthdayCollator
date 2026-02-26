using System.Text;
using BirthdayCollator.Helpers;
using BirthdayCollator.Server.Models;

namespace BirthdayCollator.Server.Processing.Cleaning;

public sealed class PersonCleaner
{

    public List<Person> CleanPersons(List<Person> people)
    {
        List<Person> cleanedPeople = [];

        foreach (Person p in people)
        {
            string cleanedName = CleanField(p.Name);
            string cleanedDescription = CleanField(p.Description);
            string finalDescription = RemoveDuplicateLeadingName(cleanedName, cleanedDescription);

            Person cleanedPerson = new()
            {
                BirthYear = p.BirthYear,
                Month = p.Month,
                Day = p.Day,
                Name = cleanedName,
                Description = finalDescription,
                Url = p.Url,
                Section = p.Section,
                SourceUrl = p.SourceUrl,
                SourceSlug = p.SourceSlug,
                DisplaySlug = p.DisplaySlug
            };

            cleanedPeople.Add(cleanedPerson);
        }

        return cleanedPeople;
    }


    private static string CleanField(string input)
    {
        string noBrackets = RemoveBracketedContent(input);
        string noCitations = RemoveCitationMarkers(noBrackets);
        return noCitations;
    }

    private static string RemoveCitationMarkers(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        string cleaned = RegexPatterns.Citation().Replace(input, "");
        return cleaned.Trim();
    }

    private static string RemoveBracketedContent(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        StringBuilder sb = new();
        int depth = 0;

        foreach (char c in input)
        {
            if (c == '(')
            {
                depth++;
                continue;
            }

            if (c == ')')
            {
                if (depth > 0)
                    depth--;
                continue;
            }

            if (depth == 0)
                sb.Append(c);
        }

        return sb.ToString().Trim();
    }

    private static string RemoveDuplicateLeadingName(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description))
            return description;

        if (description.StartsWith(name, StringComparison.OrdinalIgnoreCase))
        {
            string trimmed = description[name.Length..].TrimStart(',', ' ', '-', '–');
            return trimmed;
        }

        return description;
    }
}