using BirthdayCollator.Helpers;
using BirthdayCollator.Server.Constants;

namespace BirthdayCollator.Server.Processing.Dates;

public interface IBirthDateParser
{
    bool TryParseMonthDay(string text, int year, out DateTime result);
    bool MatchesRequestedDate(string text, DateTime date);
    string? GetYearPageDateHeader(string rawText);
    bool IsMonthName(string text);
}

public sealed class BirthDateParser : IBirthDateParser
{
    public bool TryParseMonthDay(string text, int year, out DateTime result)
    {
        result = default;

        var match = RegexPatterns.MonthDayLooseRegex().Match(text);
        if (!match.Success)
            return false;

        string monthName = match.Groups[1].Value;
        string dayString = match.Groups[2].Value;

        return DateTime.TryParse($"{monthName} {dayString} {year}", out result);
    }

    public bool MatchesRequestedDate(string text, DateTime date)
    {
        var match = RegexPatterns.MonthDayLooseRegex().Match(text);
        if (!match.Success)
            return false;

        string monthName = match.Groups[1].Value;
        string dayString = match.Groups[2].Value;

        if (!DateTime.TryParse($"{monthName} {dayString} {date.Year}", out DateTime parsed))
            return false;

        return parsed.Month == date.Month && parsed.Day == date.Day;
    }

    public string? GetYearPageDateHeader(string rawText)
    {
        var lines = rawText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var first = lines.FirstOrDefault();
        return first?.Trim();
    }

    public bool IsMonthName(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return MonthNames.All.Contains(text, StringComparer.OrdinalIgnoreCase);
    }
}
