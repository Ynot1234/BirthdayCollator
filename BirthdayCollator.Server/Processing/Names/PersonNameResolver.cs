using BirthdayCollator.Models;

namespace BirthdayCollator.Server.Processing.Names;

public interface IPersonNameResolver
{
    string ExtractName(string entryText);
    void FixSwappedName(Person person);
}

public sealed class PersonNameResolver : IPersonNameResolver
{
    public string ExtractName(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        int idx = text.IndexOf('(');
        if (idx > 0)
            text = text[..idx];

        return text.Trim().TrimEnd(',');
    }

    public void FixSwappedName(Person person)
    {
        if (person == null || string.IsNullOrWhiteSpace(person.Name))
            return;

        if (person.Name.Contains(','))
        {
            var parts = person.Name.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                string first = parts[1].Trim();
                string last = parts[0].Trim();
                person.Name = $"{first} {last}";
            }
        }
    }
}
