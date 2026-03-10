using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Html;
using BirthdayCollator.Server.Processing.Links;
using HtmlAgilityPack;
using System.Globalization;

namespace BirthdayCollator.Server.Processing.Parsers;

public sealed class GenariansPageParser(ILinkResolver linkResolver, IYearRangeProvider year) 
{
    public bool TryParseRow(HtmlNode row, string month, int day, string url, out Person? p)
    {
        p = null;
        var cells = row.SelectNodes("./th");
        var spans = cells[2].SelectNodes(".//span");

        var nameNode = spans[0].InnerText.Contains("NEW CENTENARIAN") ? spans[1] : spans[0];

        var lines = spans.Last().InnerHtml
            .Split("<br", StringSplitOptions.RemoveEmptyEntries)
            .Select(line => HtmlEntity.DeEntitize(line).Trim(' ', '\n', '\r', '>', '\t'))
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        if (lines.Count < 2 || !DateTime.TryParseExact(lines.Last(), "MM/dd/yyyy", CultureInfo.InvariantCulture, default, out var bDay))
            return false;

        if (year.IncludeAll)
        {
            var today = DateTime.Today;
            if (bDay.Month < today.Month || (bDay.Month == today.Month && bDay.Day < today.Day))
                return false; 
        }
        else
        {
            if (bDay.Day != day || !bDay.ToString("MMMM", CultureInfo.InvariantCulture).Equals(month, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        p = new Person
        {
            Name = WikiTextUtility.Normalize(nameNode.InnerText),
            Description = lines[0].Split(',')[0].Trim(),
            Url = linkResolver.ExtractWikipediaHref(cells[0]) ?? string.Empty,
            BirthYear = bDay.Year,
            Month = bDay.Month,
            Day = bDay.Day,
            SourceSlug = Path.GetFileNameWithoutExtension(url),
            SourceUrl = url,
            DisplaySlug = Constants.AppStrings.Slugs.Genarians,
            Section = Constants.AppStrings.Sections.Births};

        return true;
    }
}