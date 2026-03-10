using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Models;
using HtmlAgilityPack;
using System.Globalization;
using BirthdayCollator.Server.Helpers;
using static BirthdayCollator.Server.Constants.AppStrings;

namespace BirthdayCollator.Server.Processing.Parsers;

public sealed class OnThisDayParser
{
    public List<Person> Parse(string html, int month, int day)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(html);
        var nodes = doc.DocumentNode.SelectNodes("//li[contains(@class, 'person')]");
        if (nodes is null) return [];
        return [.. nodes.Select(node => TryParsePerson(node, month, day)).OfType<Person>()];
    }

    private static Person? TryParsePerson(HtmlNode li, int month, int day)
    {
        string raw = HtmlEntity.DeEntitize(li.InnerText).Trim();
        string? yearText = li.SelectSingleNode(".//a[contains(@class, 'birthDate')]")?.InnerText
                        ?? li.SelectSingleNode(".//b")?.InnerText;

        string cleanYear = new([.. (yearText ?? "").Where(char.IsDigit)]);
        
        if (raw.Contains("(d.") || !int.TryParse(cleanYear, out int year)) return null;
        
        string yearStr = year.ToString();
        string trimmed = raw.StartsWith(yearStr) ? raw[yearStr.Length..].Trim() : raw;
        var (name, desc) = StringNormalization.SplitNameAndDescription(trimmed);
        
        if (string.IsNullOrWhiteSpace(name)) return null;

        return new Person
        {
            Name = name,
            Description = string.IsNullOrWhiteSpace(desc) ? "Notable Person" : desc,
            BirthYear = year,
            Month = month,
            Day = day,
            SourceSlug = Slugs.OnThisDay,
            DisplaySlug = Slugs.OnThisDay,
            Section = Sections.Births,
            SourceUrl = $"{Urls.OnThisDayBase}/{CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month).ToLower()}/{day}"
        };
    }
}