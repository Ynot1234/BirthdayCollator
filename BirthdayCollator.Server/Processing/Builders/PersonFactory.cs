using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Names;
using HtmlAgilityPack;

using static BirthdayCollator.Server.Processing.Parsers.GenariansPageParser;

namespace BirthdayCollator.Server.Processing.Builders;

public class PersonFactory(Func<string, string> normalizeHref, IPersonNameResolver nameResolver)
{
    private readonly Func<string, string> _normalizeHref = normalizeHref;
    

    public Person Create(
        string name,
        string description,
        string wikiUrl,
        int birthYear,
        int month,
        int day,
        string section = "Births",
        string? sourceSlug = null)
    {
        return new Person
        {
            Id = 0,
            Name = name,
            Description = description,
            Url = wikiUrl,
            BirthYear = birthYear,
            Month = month,
            Day = day,
            Section = section,
            SourceSlug = sourceSlug
        };
    }

    public Person CreateFromParsedGenarian(ParsedGenarian parsed)
    {
        Person p = new()
        {
            Name = parsed.Name,
            Description = parsed.Description,
            Url = parsed.WikipediaUrl,

            BirthYear = parsed.BirthDate.Year,
            Month = parsed.BirthDate.Month,
            Day = parsed.BirthDate.Day,

            Section = "Births",

            SourceSlug = parsed.SourceSlug,
            DisplaySlug = "Genarians"
        };

        Person person = Create(p, parsed.SourceSlug);

        person.SourceUrl = parsed.GenariansPageUrl;

        return person;
    }

    public Person Create(Person p, string? sourceSlug)
    {
        return new Person
        {
            Name = p.Name,
            Description = p.Description,
            Url = p.Url,

            BirthYear = p.BirthYear,
            Month = p.Month,
            Day = p.Day,
            Section = p.Section,

            SourceSlug = sourceSlug,
            SourceUrl = p.SourceUrl ?? BuildWikiSourceUrl(sourceSlug),

            DisplaySlug = p.DisplaySlug
        };
    }

    public Person CreateWithSuffix(Person parsed, DateTime birthDate, string? suffix)
    {
        string idSuffix = suffix == string.Empty
            ? $"{birthDate.Year}"
            : $"{birthDate.Year}_{suffix}";

        return Create(parsed, idSuffix);
    }

    public Person Finalize(Person person)
    {
        nameResolver.FixSwappedName(person);
        return person;
    }


    public Person BuildPerson(
        string rawText,
        DateTime birthDate,
        HtmlNode personLink,
        string? sourceSlug = null)
    {
        string name = HtmlEntity.DeEntitize(personLink.InnerText).Trim();
        string description = ExtractDescription(rawText);

        string href = personLink.GetAttributeValue("href", "");
        string normalized = _normalizeHref(href);

        string wikiUrl = normalized.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? normalized
            : Urls.Domain + normalized;

        return Create(
            name,
            description,
            wikiUrl,
            birthDate.Year,
            birthDate.Month,
            birthDate.Day,
            "Births",
            sourceSlug
        );
    }



    private static string? BuildWikiSourceUrl(string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return null;

        return $"{Urls.ArticleBase}/{slug}#Births";
    }



    private static string ExtractDescription(string rawText)
    {
        int dash = rawText.IndexOf('–');

        if (dash >= 0)
            return rawText[(dash + 1)..].Trim();

        int comma = rawText.IndexOf(',');

        if (comma >= 0)
            return rawText[(comma + 1)..].Trim();

        return string.Empty;
    }
}