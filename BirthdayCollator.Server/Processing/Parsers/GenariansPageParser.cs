using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Html;
using HtmlAgilityPack;
using System.Globalization;

namespace BirthdayCollator.Server.Processing.Parsers;   
public sealed class GenariansPageParser
{
    public bool TryParseRow(HtmlNode row, string targetMonth, int targetDay, string sourceUrl, out Person? person)
    {
        person = null;
        var cells = row.SelectNodes("./th");

        if (cells is not { Count: >= 3 }) return false;

        // Use the middle cell for name/bio/date logic
        var spans = cells[2].SelectNodes(".//span");
        if (spans is not { Count: >= 2 }) return false;

        // Skip the badge if it exists
        var nameNode = spans[0].InnerText.Contains("NEW CENTENARIAN") ? spans[1] : spans[0];
        string name = WikiTextUtility.Normalize(nameNode.InnerText);

        // Extract lines and parse date
        var lines = CleanBrLines(spans.Last());
        if (lines.Count < 2 || !DateTime.TryParseExact(lines.Last(), "MM/dd/yyyy", CultureInfo.InvariantCulture, default, out var bDay))
            return false;

        // Date Filter
        if (bDay.Day != targetDay || !bDay.ToString("MMMM", CultureInfo.InvariantCulture).Equals(targetMonth, StringComparison.OrdinalIgnoreCase))
            return false;

        person = new Person
        {
            Name = name,
            Description = lines[0].Split(',')[0].Trim(),
            Url = WikipediaDomNavigator.ExtractWikipediaHref(cells[0]) ?? string.Empty,
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
            .Select(s => HtmlEntity.DeEntitize(s).Trim(' ', '\n', '\r', '>', '\t'))];
}