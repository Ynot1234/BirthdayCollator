using BirthdayCollator.Server.Helpers;
using BirthdayCollator.Server.Models;
using HtmlAgilityPack;
using System.Globalization;

namespace BirthdayCollator.Server.Processing.Parsers;
public sealed class GenariansPageParser
{
    public bool TryParseRow(HtmlNode row, string targetMonth, int targetDay, string sourceUrl, out Person? person)
    {
        person = null;

        if (row.SelectNodes("./th") is not { Count: >= 3 } cells) return false;
        if (cells[2].SelectNodes(".//span") is not { Count: >= 2 } spans) return false;

        var nameNode = spans[0].InnerText.Contains("NEW CENTENARIAN", StringComparison.Ordinal) ? spans[1] : spans[0];
        string name = StringNormalization.CleanName(HtmlEntity.DeEntitize(nameNode.InnerText).Trim());

        var lines = CleanBrLines(spans.Last());
        if (lines.Count < 2 || !DateTime.TryParseExact(lines[1], "MM/dd/yyyy", CultureInfo.InvariantCulture, default, out var bDay))
            return false;

         if (bDay.Day != targetDay || !bDay.ToString("MMMM", CultureInfo.InvariantCulture).Equals(targetMonth, StringComparison.OrdinalIgnoreCase))
            return false;

        person = new Person
        {
            Name = name,
            Description = lines[0].Split(',')[0].Trim(),
            Url = ExtractWikiUrl(cells[0]),
            BirthYear = bDay.Year,
            Month = bDay.Month,
            Day = bDay.Day,
            SourceSlug = Path.GetFileNameWithoutExtension(sourceUrl),
            SourceUrl = sourceUrl,
            DisplaySlug = "Genarians",
            Section = "Births"
        };

        return true;
    }

    private static List<string> CleanBrLines(HtmlNode node) =>
        [.. node.InnerHtml.Split("<br", StringSplitOptions.RemoveEmptyEntries)
            .Select(s => HtmlEntity.DeEntitize(HtmlNode.CreateNode($"<span>{s}</span>").InnerText)
            .Trim(' ', '\n', '\r', '>', '\t'))];

    private static string ExtractWikiUrl(HtmlNode cell)
    {
        var url = cell.SelectSingleNode(".//a[contains(@href,'wikipedia.org')]")?.GetAttributeValue("href", "") ?? "";
        return string.IsNullOrEmpty(url) ? "" : Uri.UnescapeDataString(url).Replace(" ", "_");
    }

}
