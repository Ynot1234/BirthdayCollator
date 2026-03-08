using HtmlAgilityPack;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Helpers;

namespace BirthdayCollator.Server.Processing.Validation;

public sealed class BirthEntryValidator(IYearRangeProvider yearProvider)
{
    public bool IsValidBirthEntry(string rawText, int birthYear, HtmlNode liNode)
    {
        var validYears = yearProvider.GetYears();
        if (!validYears.Contains(birthYear.ToString()))
            return false;

        if (RegexPatterns.ExcludeDiedRegex().IsMatch(rawText))
            return false;

        var links = liNode.SelectNodes(".//a[@href]");
        return links != null && links.Count > 0;
    }
}
