using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Html;
using BirthdayCollator.Server.Processing.Links;
using HtmlAgilityPack;
using System.Globalization;
using static BirthdayCollator.Server.Constants.AppStrings;

namespace BirthdayCollator.Server.Processing.Parsers;

public sealed class GenariansPageParser(ILinkResolver linkResolver, IYearRangeProvider year, PersonFactory factory) 
{
    public bool TryParseRow(HtmlNode row, string month, int day, string url, out Person? p)
    {
        p = null;
        var cells = row.SelectNodes("./th");
        if (cells == null || cells.Count < 3) return false;

        var spans = cells[2].SelectNodes(".//span");
        var nameNode = spans[0].InnerText.Contains("NEW CENTENARIAN") ? spans[1] : spans[0];

        List<string> lines = [.. spans.Last().InnerHtml
            .Split("<br", StringSplitOptions.RemoveEmptyEntries)
            .Select(line => HtmlEntity.DeEntitize(line).Trim(' ', '\n', '\r', '>', '\t'))
            .Where(s => !string.IsNullOrWhiteSpace(s))];

        if (lines.Count < 2 
        || !DateTime.TryParseExact(lines.Last(), 
                                   DateFormats.FullDate, 
                                   CultureInfo.InvariantCulture, 
                                   DateTimeStyles.None, 
                                   out var bDay))
            return false;

        if (year.IncludeAll)
        {
            var today = DateTime.Today;
            if (bDay.Month < today.Month || (bDay.Month == today.Month && bDay.Day < today.Day))
                return false;
        }
        else if (bDay.Day != day || !bDay.ToString(DateFormats.MonthLong, CultureInfo.InvariantCulture).Equals(month, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var name = WikiTextUtility.Normalize(nameNode.InnerText);
        var description = lines[0].Split(',')[0].Trim();
        var rawHref = linkResolver.ExtractWikipediaHref(cells[0]) ?? string.Empty;
        string slug = rawHref.Split('/').Last().TrimStart('.');
        string absoluteUrl = !string.IsNullOrEmpty(slug) ? $"{Constants.Urls.ArticleBase}/{slug}" : string.Empty;

        p = factory.CreatePerson(
            name: name,
            desc: description,
            birthDate: bDay,
            url: absoluteUrl,
            sourceSlug: Path.GetFileNameWithoutExtension(url),
            section: Sections.Births,
            displaySlug: slug
        );

        return true;
    }
}