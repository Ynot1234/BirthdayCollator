using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Models;
using HtmlAgilityPack;
using System.Globalization;
using BirthdayCollator.Server.Helpers;

namespace BirthdayCollator.Server.Processing.Parsers;

public sealed class OnThisDayParser
{
    public List<Person> Parse(string html, int month, int day, bool includeAll = false)
    {
        if (string.IsNullOrWhiteSpace(html))
            return [];

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var nodes = doc.DocumentNode.SelectNodes("//li[contains(@class, 'person')]");
        if (nodes is null)
            return [];

        var results = new List<Person>(nodes.Count);

        foreach (var node in nodes)
        {
            var person = TryParsePerson(node, month, day);
            if (person is not null)
                results.Add(person);
        }

        return results;
    }

    private static Person? TryParsePerson(HtmlNode li, int month, int day)
    {
        string raw = HtmlEntity.DeEntitize(li.InnerText).Trim();

        if (raw.Contains("(d.") || !TryExtractYear(li, out int year))
            return null;

        string yearStr = year.ToString();
        string trimmed = raw.StartsWith(yearStr)
            ? raw[yearStr.Length..].Trim()
            : raw;

        var (name, desc) = StringNormalization.SplitNameAndDescription(trimmed);

        if (string.IsNullOrWhiteSpace(name))
            return null;

        return new Person
        {
            Name = name,
            Description = string.IsNullOrWhiteSpace(desc) ? "Notable Person" : desc,
            BirthYear = year,
            Month = month,
            Day = day,
            Url = string.Empty,//WikiEnricher fills this later
            SourceSlug = "OnThisDay",
            DisplaySlug = "OnThisDay",
            Section = "Births",
            SourceUrl = $"{Urls.OnThisDayBase}/{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month).ToLower()}/{day}"
        };
    }

    private static bool TryExtractYear(HtmlNode li, out int year)
    {
        string? yearText =
            li.SelectSingleNode(".//a[contains(@class, 'birthDate')]")?.InnerText
            ?? li.SelectSingleNode(".//b")?.InnerText;

        string cleanYear = new(yearText?.Where(char.IsDigit).ToArray());

        return int.TryParse(cleanYear, out year);
    }
}