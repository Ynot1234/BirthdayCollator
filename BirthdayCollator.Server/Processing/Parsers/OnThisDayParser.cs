using BirthdayCollator.Server.Constants;
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

            var (name, desc) = SplitNameAndDescription(trimmed);

            if (string.IsNullOrWhiteSpace(name))
                continue;

            results.Add(personFactory.CreatePerson(
                name: name,
                desc: desc,
                birthDate: new DateTime(year, month, day),
                url: string.Empty,
                sourceSlug: AppStrings.Slugs.OnThisDay,
                section: AppStrings.Sections.Births,
                displaySlug: name.Replace(" ", "_")
            ));
        }

        return results;
    }

    private static (string name, string? description) SplitNameAndDescription(string text)
    {
        string cleaned = text.TrimStart(' ', ',', '-', '–');

        int commaIndex = cleaned.IndexOf(',');
        if (commaIndex < 0) return (cleaned.Trim(), null);

        return (
            cleaned[..commaIndex].Trim(),
            cleaned[(commaIndex + 1)..].Trim()
        );
    }
}