using HtmlAgilityPack;
using System.Globalization;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Validation;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Html;
using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Processing.Links;

namespace BirthdayCollator.Server.Processing.Parsers;

public sealed partial class DatePageParser(BirthEntryValidator validator, PersonFactory personFactory) :IDatePageParser
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

            HtmlNode? personLink = LinkResolver.TryFindPersonLink(li);
            
            if (personLink == null)
                continue;

            DateTime birthDate = new(birthYear, month, day);

            Person person = personFactory.BuildPerson(rawText, birthDate, personLink);

            person.SourceSlug = sourceSlug;
            person.SourceUrl = $"{Urls.ArticleBase}/{sourceSlug}#Births";
            person = personFactory.Finalize(person);
            results.Add(person);
        }

        return results;
    }
}