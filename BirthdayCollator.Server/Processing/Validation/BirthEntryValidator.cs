using HtmlAgilityPack;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BirthdayCollator.Server.Processing.Validation
{
    public class BirthEntryValidator(
        HashSet<string> validYearSet,
        Regex excludeDiedRegex)
    {
        private readonly HashSet<string> _validYearSet = validYearSet;
        private readonly Regex _excludeDiedRegex = excludeDiedRegex;

        public bool IsValidBirthEntry(
            string rawText,
            int birthYear,
            HtmlNode liNode)
        {
            if (!_validYearSet.Contains(birthYear.ToString()))
                return false;

            if (_excludeDiedRegex.IsMatch(rawText))
                return false;


            HtmlNodeCollection links = liNode.SelectNodes(".//a[@href]");

            if (links == null || links.Count == 0)
                return false;

            return true;
        }
    }
}
