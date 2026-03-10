using BirthdayCollator.Server.Helpers;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using HtmlAgilityPack;

namespace BirthdayCollator.Server.Processing.Parsers;

public sealed class OnThisDayParser(PersonFactory personFactory)
{
    public List<Person> Parse(string html, int month, int day)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(html);

        var nodes = doc.DocumentNode.SelectNodes("//li[contains(@class, 'person')]");
        if (nodes is null) return [];

        List<Person> results = [];

        foreach (var li in nodes)
        {
            string raw = HtmlEntity.DeEntitize(li.InnerText).Trim();

            string? yearText = li.SelectSingleNode(".//a[contains(@class, 'birthDate')]")?.InnerText
                            ?? li.SelectSingleNode(".//b")?.InnerText;

            string cleanYear = new([.. (yearText ?? "").Where(char.IsDigit)]);

            if (raw.Contains("(d.") || !int.TryParse(cleanYear, out int year))
                continue;

            string yearStr = year.ToString();
            string trimmed = raw.StartsWith(yearStr) ? raw[yearStr.Length..].Trim() : raw;
            var (name, desc) = StringNormalization.SplitNameAndDescription(trimmed);

            if (string.IsNullOrWhiteSpace(name))
                continue;

            results.Add(personFactory.BuildOnThisDay(name, desc, year, month, day));
        }

        return results;
    }
}