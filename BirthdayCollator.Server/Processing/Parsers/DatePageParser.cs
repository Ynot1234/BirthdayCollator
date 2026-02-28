using BirthdayCollator.Helpers;
using HtmlAgilityPack;
using System.Globalization;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Validation;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Constants;

namespace BirthdayCollator.Server.Processing.Parsers;

public sealed partial class DatePageParser(BirthEntryValidator validator, PersonFactory personFactory)
{

    public List<Person> Parse(string html, int month, int day)
    {
        HtmlDocument htmlDoc = new();
        
        htmlDoc.LoadHtml(html);
        
        List<HtmlNode> liNodes = ExtractBirthLiNodes(htmlDoc);
    
        if (liNodes.Count == 0)
            return [];

        List<Person> results = [];

        foreach (HtmlNode li in liNodes)
        {
            string rawText = NormalizeRawText(HtmlEntity.DeEntitize(li.InnerText));

            if (!rawText.Contains('–'))
                continue;

            if (!TryExtractBirthYearFromDatePage(rawText, out int birthYear))
                continue;
            
            if (!validator.IsValidBirthEntry(rawText, birthYear, li))
                continue;

            HtmlNode? personLink = TryFindPersonLink(li);

            if (personLink == null)
                continue;

            DateTime birthDate = new(birthYear, month, day);
            Person parsed = personFactory.BuildPerson(rawText, birthDate, personLink);
            string monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month);
            Person person = personFactory.Create(parsed, $"{monthName} {day}");
            results.Add(person);
        }

        return results;
    }

    public static string Normalize(string? input) => string.IsNullOrWhiteSpace(input) ? string.Empty : input;


    private static string NormalizeRawText(string input)
    {
        input = Normalize(HtmlEntity.DeEntitize(input));
        string cleaned = input.Trim();
        cleaned = cleaned.Replace('—', '–').Replace('-', '–');
        cleaned = RegexPatterns.WhitespaceCollapseRegex().Replace(cleaned, " ");
        cleaned = RegexPatterns.OrdinalSuffixRegex().Replace(cleaned, "$1");
        return cleaned;
    }

    private static List<HtmlNode> ExtractBirthLiNodes(HtmlDocument htmlDoc)
    {
        HtmlNode birthsHeader = htmlDoc.DocumentNode.SelectSingleNode(XPathSelectors.YearBirthsHeader);

        if (birthsHeader == null)
            return [];

        List<HtmlNode> sections = [];

        for (HtmlNode node = birthsHeader.NextSibling;
             node != null;
             node = node.NextSibling)
        {
            if (node.Name == "h2")
                break;

            if (node.Name is "section" or "div" or "ul")
                sections.Add(node);
        }

        List<HtmlNode> allListItems = [.. sections
                                       .SelectMany(section =>
                                       {
                                           HtmlNodeCollection items = section.SelectNodes(".//li");
                                           return items ?? Enumerable.Empty<HtmlNode>();
                                       })];     

        return allListItems;

    }

    private static bool IsYearLink(string href)
    {
        string trimmed = href.TrimStart('.', '/');
        return trimmed.Length == 4 && int.TryParse(trimmed, out _);
    }

    private static bool IsMonthDayLink(string href)
    {
        string[] parts = href.Split('_');
        if (parts.Length != 2)
            return false;

        if (!DateTime.TryParse($"{parts[0]} 1", out _))
            return false;

        return int.TryParse(parts[1], out int day) && day is >= 1 and <= 31;
    }

    private static HtmlNode? TryFindPersonLink(HtmlNode liNode)
    {
        HtmlNodeCollection links = liNode.SelectNodes(XPathSelectors.DescendantAnchorHref);

        if (links == null || links.Count == 0)
            return null;

        foreach (HtmlNode link in links)
        {
            string href = link.GetAttributeValue("href", "");

            if (IsYearLink(href))
                continue;

            if (IsMonthDayLink(href))
                continue;

            return link;
        }

        return null;
    }

    private static bool TryExtractBirthYearFromDatePage(string rawText, out int birthYear)
    {
        birthYear = 0;
        string[] parts = rawText.Split('–', 2);
        
        if (parts.Length < 2)
            return false;

        string leftSide = parts[0].Trim();
        string[] tokens = leftSide.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length == 0)
            return false;

        string yearCandidate = tokens[^1];
        return int.TryParse(yearCandidate, out birthYear);
    }
}