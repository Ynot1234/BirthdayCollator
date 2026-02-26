using System.Globalization;
using HtmlAgilityPack;

namespace BirthdayCollator.Server.Processing.Parsers;

public sealed class GenariansPageParser()
{

    public sealed record ParsedGenarian(
                                        string Name,
                                        string Description,
                                        string WikipediaUrl,
                                        DateTime BirthDate,
                                        string GenariansPageUrl,
                                        string SourceSlug);

    public bool TryParseRow(
        HtmlNode row,
        string? targetMonthName,
        int? targetDay,
        string sourceUrl,
        out ParsedGenarian parsed)
    {
        parsed = null!;

        // Validate row structure
        HtmlNodeCollection cells = row.SelectNodes("./th");
        if (cells == null || cells.Count < 3)
            return false;

        // Extract Wikipedia link
        string wikiUrl = ExtractWikipediaUrl(cells[0]);

        // Extract spans
        HtmlNodeCollection? spans = GetSpans(cells[2]);
        if (spans == null || spans.Count < 2)
            return false;

        // Extract name
        string name = ExtractName(spans);

        // Extract description + date string
        var (description, dateString) = ExtractDescriptionAndDate(spans);

        // Parse date
        if (!TryParseGenarianDate(dateString, out var parsedDate))
            return false;

        // Filter by requested month/day
        if (!MatchesTargetDate(parsedDate, targetMonthName, targetDay))
            return false;

        // Build birth date
        DateTime birthDate = new(parsedDate.Year, parsedDate.Month, parsedDate.Day);

        parsed = new ParsedGenarian(  Name: name,
                                      Description: description,
                                      WikipediaUrl: wikiUrl,
                                      BirthDate: birthDate,
                                      GenariansPageUrl: sourceUrl,
                                      SourceSlug: ExtractGenarianSlug(sourceUrl));
        return true;
    }




    private static string ExtractGenarianSlug(string url)
    {
        string file = Path.GetFileNameWithoutExtension(url);
        return file;
    }

    private static List<string> SplitLines(HtmlNode span)
    {
        List<string> lines = [.. span.InnerHtml
                    .Split("<br>", StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => HtmlEntity.DeEntitize(s.Trim()))];

        return lines;
    }

    private static string StripHtml(string input)
    {
        string node = HtmlNode.CreateNode($"<span>{input}</span>").InnerText;
        return HtmlEntity.DeEntitize(node).Trim();
    }

    private static string ExtractPrimaryDescription(string raw)
    {
        string noHtml = StripHtml(raw);
        int commaIndex = noHtml.IndexOf(',');

        return commaIndex > 0
            ? noHtml[..commaIndex].Trim()
            : noHtml;
    }


    private static bool MatchesTargetDate(  DateTime parsed,
                                            string? targetMonthName,
                                            int? targetDay)
    {
        if (targetMonthName is null || targetDay is null)
            return true;

        return parsed.ToString("MMMM", CultureInfo.InvariantCulture)
                     .Equals(targetMonthName, StringComparison.OrdinalIgnoreCase)
            && parsed.Day == targetDay;
    }


    private static bool TryParseGenarianDate(string date, out DateTime parsed)
    {
        return DateTime.TryParseExact(
            date,
            "MM/dd/yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out parsed);
    }


    private static (string Description, string Date)
        ExtractDescriptionAndDate(HtmlNodeCollection spans)
    {
        HtmlNode lastSpan = spans.Last();

        List<string> lines = SplitLines(lastSpan);

        string description = lines.Count > 0
            ? ExtractPrimaryDescription(lines[0])
            : "";

        string date = lines.Count > 1
            ? lines[1]
            : "";

        return (description, date);
    }

    private static string ExtractName(HtmlNodeCollection spans)
    {
        string first = spans[0].InnerText.Trim();

        int index = IsNewCentenarian(first) ? 1 : 0;

        return spans[index].InnerText.Trim();
    }

    private static HtmlNodeCollection? GetSpans(HtmlNode infoCell) => infoCell.SelectNodes(".//span");

    private static bool IsNewCentenarian(string text) => text.Contains("NEW CENTENARIAN", StringComparison.InvariantCultureIgnoreCase);


    private static string ExtractWikipediaUrl(HtmlNode cell)
    {
        string url = cell.SelectSingleNode(".//a[contains(@href,'wikipedia.org')]")
                         ?.GetAttributeValue("href", "") ?? "";

        return Uri.UnescapeDataString(url).Replace(" ", "_");
    }
}