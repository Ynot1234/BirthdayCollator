using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Html;
using BirthdayCollator.Server.Processing.Names;
using HtmlAgilityPack;

namespace BirthdayCollator.Server.Processing.Builders;

public class PersonFactory(Func<string, string> normalizeHref, IPersonNameResolver nameResolver)
{
    public Person BuildPerson(string rawText, DateTime birthDate, HtmlNode? personLink, string? sourceSlug = null)
    {
        string href = personLink?.GetAttributeValue("href", "") ?? string.Empty;

        string name = personLink != null
            ? HtmlEntity.DeEntitize(personLink.InnerText).Trim()
            : WikiTextUtility.ExtractPersonName(rawText);

        string normalized = string.IsNullOrEmpty(href) ? "" : normalizeHref(href);

        string url = string.IsNullOrEmpty(normalized)
            ? ""
            : normalized.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                ? normalized
                : Urls.Domain + normalized;

        string desc = WikiTextUtility.ExtractDescription(rawText);

        return new Person
        {
            Name = name,
            Description = desc,
            Url = url,
            BirthYear = birthDate.Year,
            Month = birthDate.Month,
            Day = birthDate.Day,
            Section = "Births",
            SourceSlug = sourceSlug,
            SourceUrl = sourceSlug != null ? $"{Urls.ArticleBase}/{sourceSlug}#Births" : null
        };
    }


    public Person Finalize(Person person)
    {
        nameResolver.FixSwappedName(person);
        return person;
    }
}