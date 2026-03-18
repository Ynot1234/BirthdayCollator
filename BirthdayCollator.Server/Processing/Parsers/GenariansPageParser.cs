using System.Globalization;

namespace BirthdayCollator.Server.Processing.Parsers;

public sealed class GenariansPageParser(ILinkResolver linkResolver, IYearRangeProvider year, PersonFactory factory)
{
    public bool TryParseRow(HtmlNode row, string month, int day, string url, out Person? p)
    {
        p = null;
        var cells = row.SelectNodes("./th");

        if (cells is null or { Count: < 3 }) return false;

        var spans = cells[2].SelectNodes(".//span");
        if (spans is null or { Count: 0 }) return false;

        var nameNode = spans[0].InnerText.Contains("NEW CENTENARIAN", StringComparison.OrdinalIgnoreCase)
            ? spans[1]
            : spans[0];

        var lastSpanHtml = spans.Last().InnerHtml;
        string[] rawLines = lastSpanHtml.Split("<br", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        List<string> lines = [];
        foreach (var line in rawLines)
        {
            string clean = HtmlEntity.DeEntitize(line).Trim(' ', '\n', '\r', '>', '\t');
            if (!string.IsNullOrWhiteSpace(clean))
            {
                lines.Add(clean);
            }
        }

        if (lines.Count < 2 || !DateTime.TryParseExact(lines.Last(),
                DateFormats.FullDate,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var bDay))
        {
            return false;
        }

        if (!IsDateRelevant(bDay, month, day))
        {
            return false;
        }

        var name = WikiTextUtility.Normalize(nameNode.InnerText);

        ReadOnlySpan<char> firstLine = lines[0].AsSpan();
        int commaIndex = firstLine.IndexOf(',');
        string description = (commaIndex == -1 ? firstLine : firstLine[..commaIndex]).Trim().ToString();

        var rawHref = linkResolver.ExtractWikipediaHref(cells[0]) ?? string.Empty;

        ReadOnlySpan<char> hrefSpan = rawHref.AsSpan();
        int lastSlash = hrefSpan.LastIndexOf('/');
        string slug = lastSlash == -1
            ? rawHref.TrimStart('.')
            : new string(hrefSpan[(lastSlash + 1)..].TrimStart('.'));

        string absoluteUrl = !string.IsNullOrEmpty(slug) ? $"{Urls.ArticleBase}/{slug}" : string.Empty;

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

    private bool IsDateRelevant(DateTime bDay, string targetMonth, int targetDay)
    {
        if (year.IncludeAll)
        {
            var today = DateTime.Today;
            return bDay.Month > today.Month 
               || (bDay.Month == today.Month && bDay.Day >= today.Day);
        }

        return bDay.Day == targetDay &&
               bDay.ToString(DateFormats.MonthLong, CultureInfo.InvariantCulture)
                   .Equals(targetMonth, StringComparison.OrdinalIgnoreCase);
    }
}