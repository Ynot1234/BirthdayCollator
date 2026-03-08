using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Models;
using HtmlAgilityPack;
using System.Globalization;
using BirthdayCollator.Server.Helpers;

namespace BirthdayCollator.Server.Processing.Parsers;
public sealed class OnThisDayParser
{
    public List<Person> Parse(string html, int month, int day)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var nodes = doc.DocumentNode.SelectNodes("//li[@class='person']");
        if (nodes == null) return [];

        return [.. nodes
            .Select(node => TryParsePerson(node, month, day, out var p) ? p : null)
            .Where(p => p != null)
            .Cast<Person>()];
    }

    private static bool TryParsePerson(HtmlNode li, int m, int d, out Person? person)
    {
        person = null;
        string raw = HtmlEntity.DeEntitize(li.InnerText).Trim();

        if (raw.Contains("(d.") || !TryExtractYear(li, out int year)) return false;

        string trimmed = raw.StartsWith(year.ToString()) ? raw[year.ToString().Length..].Trim() : raw;
        var (name, desc) = StringNormalization.SplitNameAndDescription(trimmed);

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(desc)) return false;

        person = new Person
        {
            Name = name,
            Description = desc,
            BirthYear = year,
            Month = m,
            Day = d,
            Url = string.Empty,
            SourceSlug = "OnThisDay",
            DisplaySlug = "OnThisDay",
            Section = "Births",
            SourceUrl = $"{Urls.OnThisDayBase}/{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(m).ToLower()}/{d}"
        };

        return true;
    }

    private static bool TryExtractYear(HtmlNode li, out int year)
    {
        string? yearText = li.SelectSingleNode(".//a[@class='birthDate']")?.InnerText
                        ?? li.SelectSingleNode(".//b")?.InnerText;

        return int.TryParse(yearText?.Trim(), out year);
    }

}