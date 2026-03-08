using HtmlAgilityPack;
using System.Globalization;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Validation;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Html;
using BirthdayCollator.Server.Constants;

namespace BirthdayCollator.Server.Processing.Parsers;

public sealed partial class DatePageParser(BirthEntryValidator validator, PersonFactory personFactory)
{
    public List<Person> Parse(string html, int month, int day)
    {
        HtmlDocument htmlDoc = new();
        htmlDoc.LoadHtml(html);

        List<HtmlNode> liNodes = WikipediaDomNavigator.ExtractBirthLiNodes(htmlDoc);
        if (liNodes.Count == 0) return [];

        List<Person> results = [];
        string monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month);
        string sourceSlug = $"{monthName}_{day}";

        foreach (HtmlNode li in liNodes)
        {
            string rawText = WikiTextUtility.Normalize(li.InnerText);

            if (!rawText.Contains('–') ||
                !WikiTextUtility.TryExtractBirthYear(rawText, out int birthYear))
                continue;

            if (!validator.IsValidBirthEntry(rawText, birthYear, li))
                continue;

            HtmlNode? personLink = WikipediaDomNavigator.TryFindPersonLink(li);
            if (personLink == null)
                continue;

            DateTime birthDate = new(birthYear, month, day);

            // Build the base person
            Person person = personFactory.BuildPerson(rawText, birthDate, personLink);

            // Apply page-specific context
            person.SourceSlug = sourceSlug;
            person.SourceUrl = $"{Urls.ArticleBase}/{sourceSlug}#Births";

            // Run name fixes and add to results
            results.Add(personFactory.Finalize(person));
        }

        return results;
    }
}
