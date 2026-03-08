using HtmlAgilityPack;
using System.Text.RegularExpressions;
using BirthdayCollator.Server.Constants;

namespace BirthdayCollator.Server.Processing.Validation;

public class BirthEntryValidator(HashSet<string> validYearSet, Regex excludeDiedRegex)
{
    public bool IsValidBirthEntry(string rawText, int birthYear, HtmlNode liNode)
    {
        if (!validYearSet.Contains(birthYear.ToString()))
            return false;

        if (excludeDiedRegex.IsMatch(rawText))
            return false;


        HtmlNodeCollection links = liNode.SelectNodes(XPathSelectors.DescendantAnchorHref);

        if (links == null || links.Count == 0)
            return false;

        return true;
    }
}