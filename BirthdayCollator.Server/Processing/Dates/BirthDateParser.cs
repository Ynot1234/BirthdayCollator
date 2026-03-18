using static System.Globalization.CultureInfo;

namespace BirthdayCollator.Server.Processing.Dates;

public sealed class BirthDateParser : IBirthDateParser
{
    private static bool TryExtractMonthDay(string text, out ReadOnlySpan<char> month, out ReadOnlySpan<char> day)
    {
        var match = RegexPatterns.MonthDayLoose().Match(text);
        if (match.Success)
        {
            month = match.Groups[1].ValueSpan;
            day = match.Groups[2].ValueSpan;
            return true;
        }

        month = day = default;
        return false;
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

        if (!TryExtractMonthDay(text, out var m, out var d))
            return false;

        Span<char> dateBuffer = stackalloc char[64];
        int written = 0;

        m.CopyTo(dateBuffer);
        written += m.Length;
        dateBuffer[written++] = ' ';

        d.CopyTo(dateBuffer[written..]);
        written += d.Length;
        dateBuffer[written++] = ' ';

        if (!year.TryFormat(dateBuffer[written..], out int yearLen))
            return false;

        written += yearLen;

        return DateTime.TryParse(dateBuffer[..written], InvariantCulture, out result);
    }

    public bool MatchesRequestedDate(string text, DateTime date)
    {
        return TryParseMonthDay(text, date.Year, out var parsed)
               && parsed.Month == date.Month && parsed.Day == date.Day;
    }

    public string? GetYearPageDateHeader(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return null;

        ReadOnlySpan<char> span = rawText.AsSpan().Trim();
        int newlineIdx = span.IndexOf('\n');

        ReadOnlySpan<char> firstLine = newlineIdx == -1
            ? span
            : span[..newlineIdx].Trim();

        return firstLine.IsEmpty ? null : new string(firstLine);
    }

    public bool IsMonthName(string text) =>
        !string.IsNullOrWhiteSpace(text) && MonthNames.All.Contains(text, StringComparer.OrdinalIgnoreCase);
}