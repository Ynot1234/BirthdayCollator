using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Html;
using BirthdayCollator.Server.Processing.Names;
using HtmlAgilityPack;

namespace BirthdayCollator.Server.Processing.Builders;
public class PersonFactory(Func<string, string> normalizeHref, IPersonNameResolver nameResolver)
{
    public Person BuildFromWikiRow(string rawText, DateTime birthDate, HtmlNode? personLink, string? sourceSlug = null)
    {
        string href = personLink?.GetAttributeValue("href", "") ?? string.Empty;

        string name = personLink switch
        {
            { } link => HtmlEntity.DeEntitize(link.InnerText).Trim(), _ => WikiTextUtility.ExtractPersonName(rawText)
        };

        string normalized = string.IsNullOrEmpty(href) ? "" : normalizeHref(href);

        string url = normalized switch
        {
            "" => "",
            var h when h.StartsWith("http", StringComparison.OrdinalIgnoreCase) => h,
            _ => Urls.Domain + normalized
        };

        return CreatePerson(name, WikiTextUtility.ExtractDescription(rawText), birthDate,  url, sourceSlug, AppStrings.Sections.Births, sourceSlug);
    }

    public Person BuildOnThisDay(string name, string? desc, int year, int month, int day) =>
     CreatePerson(
         name,
         desc,
         new DateTime(year, month, day),
         Urls.GetOnThisDayUrl(month, day),
         AppStrings.Slugs.OnThisDay,
         String.Empty,
         AppStrings.Slugs.OnThisDay
     );

    public Person CreatePerson(string name, string? desc, DateTime birthDate, string url, string? sourceSlug, string section, string? displaySlug = null) =>
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