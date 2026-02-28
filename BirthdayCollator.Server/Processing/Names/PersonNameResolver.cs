using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Helpers;

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
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        string formatted = StringNormalization.RemoveParenthetical(text);
        return StringNormalization.CleanName(formatted);
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
