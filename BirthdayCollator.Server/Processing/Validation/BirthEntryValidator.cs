using BirthdayCollator.Server.Processing.Builders;

namespace BirthdayCollator.Server.Processing.Validation;

public sealed class BirthEntryValidator(IYearRangeProvider yearProvider)
{
    public bool IsValidBirthEntry(string rawText, int birthYear, int month, int day, HtmlNode liNode)
    {
        IReadOnlyList<string> validYears = yearProvider.GetYears();

        if (!validYears.Contains(birthYear.ToString()))
            return false;

        if (yearProvider.IncludeAll)
        {
            var today = DateTime.Today;

            if (month < today.Month 
            || (month == today.Month && day < today.Day))
                return false;
        }

        if (RegexPatterns.ExcludeDied().IsMatch(rawText))
            return false;

        var links = liNode.SelectNodes(XPathSelectors.DescendantAnchorHref);
        return links != null && links.Count > 0;
    }
}