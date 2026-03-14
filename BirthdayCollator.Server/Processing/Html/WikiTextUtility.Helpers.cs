using BirthdayCollator.Helpers;
using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Helpers;
using System.Text.RegularExpressions;

namespace BirthdayCollator.Server.Processing.Html
{
    public static partial class WikiTextUtility
    {

        private static List<string> SplitAndCleanLines(string rawText) =>
          [.. rawText.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
               .Select(l => l.Trim())
               .Where(l => !string.IsNullOrWhiteSpace(l))];

        private static string SelectTargetLine(List<string> lines, string fallback)
        {
            string? line = lines.FirstOrDefault(l =>
                !MonthNames.All.Any(m =>
                    l.Equals(m, StringComparison.OrdinalIgnoreCase) ||
                    (l.StartsWith(m, StringComparison.OrdinalIgnoreCase) && l.Length < m.Length + 5)
                ));

            return line ?? lines.FirstOrDefault() ?? fallback;
        }

        private static string ExtractCoreDescription(string line)
        {
            //int lastMeta = Math.Max(line.LastIndexOf(']'), line.LastIndexOf(')'));

            //if (lastMeta >= 0 && lastMeta < line.Length - 1)
            //    return line[(lastMeta + 1)..].TrimDebris();

            //int dash = line.IndexOfAny(['–', '-']);
            //return dash >= 0
            //    ? line[(dash + 1)..].TrimDebris()
            //    : line.TrimDebris();


            int prefixMeta = Math.Max(line.IndexOf(']'), line.IndexOf(')'));

            if (prefixMeta >= 0 && prefixMeta < 25)
                return line[(prefixMeta + 1)..].TrimDebris();

            // Only chop at a dash if it's also near the start
            int dash = line.IndexOfAny(['–', '-']);
            if (dash >= 0 && dash < 25)
                return line[(dash + 1)..].TrimDebris();

            return line.TrimDebris();
        }



        private static string RemovePersonName(string description, string? personName)
        {
            if (string.IsNullOrEmpty(personName))
                return description;

            return Regex.Replace(description, Regex.Escape(personName), "", RegexOptions.IgnoreCase)
                        .TrimDebris();
        }

        private static string RemoveTitles(string description)
        {
            foreach (var title in NameParsing.Titles)
            {
                if (description.Contains(title, StringComparison.OrdinalIgnoreCase))
                {
                    description = description.Replace(title, "", StringComparison.OrdinalIgnoreCase)
                                             .TrimDebris();
                }
            }
            return description;
        }

        private static string RemovePrefixes(string description)
        {
            foreach (var prefix in NameParsing.Prefixes)
            {
                if (description.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return description[prefix.Length..].TrimDebris();
            }
            return description;
        }

        private static string StripLeadingVerbs(string description)
        {
            if (description.StartsWith("is ", StringComparison.OrdinalIgnoreCase) && description.Length > 3)
                return description[3..].TrimDebris();

            if (description.StartsWith("was ", StringComparison.OrdinalIgnoreCase) && description.Length > 4)
                return description[4..].TrimDebris();

            return description;
        }

        private static string ExtractFirstSentence(string description)
        {
            var match = RegexPatterns.SentenceBoundary().Match(description);

            return match.Success
                ? description[..match.Index].TrimDebris()
                : description.TrimDebris();
        }
    }
}
