using BirthdayCollator.Helpers;
using BirthdayCollator.Server.Constants;

namespace BirthdayCollator.Server.Processing.Dates;

public sealed class BirthDateParser : IBirthDateParser
{
    private static (string Month, string Day)? ExtractMonthDay(string text)
    {
        var match = RegexPatterns.MonthDayLooseRegex().Match(text);
        return match.Success ? (match.Groups[1].Value, match.Groups[2].Value) : null;
    }

    public bool IsOnOrAfterDate(string entry, DateTime targetDate)
    {
        if (!TryParseMonthDay(entry, targetDate.Year, out var parsed))
            return false;

        if (parsed.Month > targetDate.Month) return true;

        return parsed.Month == targetDate.Month && parsed.Day >= targetDate.Day;
    }

    public bool TryParseMonthDay(string text, int year, out DateTime result)
    {
        result = default;
        var parts = ExtractMonthDay(text);

        return parts != null && DateTime.TryParse($"{parts.Value.Month} {parts.Value.Day} {year}",
            System.Globalization.CultureInfo.InvariantCulture, out result);
    }

    public bool MatchesRequestedDate(string text, DateTime date)
    {
        return TryParseMonthDay(text, date.Year, out var parsed)
               && parsed.Month == date.Month && parsed.Day == date.Day;
    }

    public string? GetYearPageDateHeader(string rawText) =>
        rawText.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();

    public bool IsMonthName(string text) =>
        !string.IsNullOrWhiteSpace(text) && MonthNames.All.Contains(text, StringComparer.OrdinalIgnoreCase);
}