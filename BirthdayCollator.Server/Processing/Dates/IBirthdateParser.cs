namespace BirthdayCollator.Server.Processing.Dates;

public interface IBirthDateParser
{
    bool MatchesRequestedDate(string text, DateTime date);

    bool IsOnOrAfterDate(string entry, DateTime targetDate);

    bool TryParseMonthDay(string text, int year, out DateTime result);

    bool IsMonthName(string text);

    string? GetYearPageDateHeader(string rawText);
}