using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Html;

namespace BirthdayCollator.Server.Processing.Cleaning;

public sealed class PersonCleaner
{
    public List<Person> CleanPersons(List<Person> people) =>
      [.. people
          .Select(p =>
          {
              var cleaned = p.Clone(); 
              cleaned.Name = WikiTextUtility.SanitizeWikiText(cleaned.Name);
              cleaned.Description = WikiTextUtility.SanitizeWikiText(cleaned.Description);
              return cleaned;
          })
          .Where(p => !string.IsNullOrWhiteSpace(p.Description))];

}
