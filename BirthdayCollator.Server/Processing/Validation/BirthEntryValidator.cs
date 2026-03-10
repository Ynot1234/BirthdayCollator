using BirthdayCollator.Helpers;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Processing.Html;
using HtmlAgilityPack;

namespace BirthdayCollator.Server.Processing.Validation;

public sealed class BirthEntryValidator(IYearRangeProvider yearProvider)
{
    public bool IsValidBirthEntry(string rawText, int birthYear, int month, int day, HtmlNode liNode)
    {
        var validYears = yearProvider.GetYears();
        if (!validYears.Contains(birthYear.ToString()))
            return false;

        if (yearProvider.IncludeAll)
        {
            var today = DateTime.Today;

            // only allow birthdays from today → end of year
            if (month < today.Month || (month == today.Month && day < today.Day))
                return false;
        }

        if (RegexPatterns.ExcludeDied().IsMatch(rawText))
            return false;

        var links = liNode.SelectNodes(".//a[@href]");
        return links != null && links.Count > 0;
    }
}

