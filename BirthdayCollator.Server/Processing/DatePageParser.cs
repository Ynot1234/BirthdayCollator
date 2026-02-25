using BirthdayCollator.Models;
using BirthdayCollator.Helpers;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace BirthdayCollator.Processing;

public sealed partial class DatePageParser(BirthEntryValidator validator, PersonFactory personFactory)
{

    public List<Person> Parse(string html, int month, int day)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);
        
        var liNodes = ExtractBirthLiNodes(htmlDoc);
        if (liNodes.Count == 0)
            return [];

        var results = new List<Person>();

        foreach (var li in liNodes)
        {
            string rawText = NormalizeRawText(HtmlEntity.DeEntitize(li.InnerText));

            if (!ContainsDashSeparator(rawText))
                continue;

            if (!TryExtractBirthYearFromDatePage(rawText, out int birthYear))
                continue;
            
            if (!validator.IsValidBirthEntry(rawText, birthYear, li))
                continue;

            HtmlNode? personLink = TryFindPersonLink(li);

            if (personLink == null)
                continue;

            string monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month);
            DateTime birthDate = new(birthYear, month, day);
            Person parsed = personFactory.BuildPerson(rawText, birthDate, personLink);
            Person person = personFactory.Create(parsed, $"{monthName} {day}");
            results.Add(person);
        }

        return results;
    }


    private static string NormalizeRawText(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        string cleaned = input.Trim();
        cleaned = cleaned.Replace('—', '–').Replace('-', '–');
        cleaned = RegexPatterns.WhitespaceCollapseRegex().Replace(cleaned, " ");
        cleaned = RegexPatterns.OrdinalSuffixRegex().Replace(cleaned, "$1");
        return cleaned;
    }


    private static bool ContainsDashSeparator(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return text.Contains('–');
    }


    private static List<HtmlNode> ExtractBirthLiNodes(HtmlDocument htmlDoc)
    {
        var birthsHeader = htmlDoc.DocumentNode.SelectSingleNode("//h2[@id='Births']");
        if (birthsHeader == null)
            return [];

        var sections = new List<HtmlNode>();

        for (HtmlNode node = birthsHeader.NextSibling;
             node != null;
             node = node.NextSibling)
        {
            if (node.Name == "h2")
                break;

            if (node.Name is "section" or "div" or "ul")
                sections.Add(node);
        }

        return [.. sections.SelectMany(s => s.SelectNodes(".//li") ?? Enumerable.Empty<HtmlNode>())];
    }

    private static bool IsYearLink(string href)
    {
        string trimmed = href.TrimStart('.', '/');
        return trimmed.Length == 4 && int.TryParse(trimmed, out _);
    }

    private static bool IsMonthDayLink(string href)
    {
        var parts = href.Split('_');
        if (parts.Length != 2)
            return false;

        if (!DateTime.TryParse($"{parts[0]} 1", out _))
            return false;

        return int.TryParse(parts[1], out int day) && day is >= 1 and <= 31;
    }

    private static HtmlNode? TryFindPersonLink(HtmlNode liNode)
    {
        HtmlNodeCollection links = liNode.SelectNodes(".//a[@href]");
        if (links == null || links.Count == 0)
            return null;

        foreach (var link in links)
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