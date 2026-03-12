using BirthdayCollator.Helpers;
using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Helpers;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace BirthdayCollator.Server.Processing.Html;

public static partial class WikiTextUtility
{
    public static string ExtractDescription(string rawText, string? personName = null)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return string.Empty;

        var lines = SplitAndCleanLines(rawText);
        string targetLine = SelectTargetLine(lines, rawText);

        string description = ExtractCoreDescription(targetLine);
        description = RemovePersonName(description, personName);
        description = RemoveTitles(description);
        description = RemovePrefixes(description);
        description = StripLeadingVerbs(description);

        string final = ExtractFirstSentence(description);

        return string.IsNullOrWhiteSpace(final) || final.Length < 3
            ? targetLine.Trim()
            : final;
    }

    public static string? GetFirstBioParagraph(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        HtmlDocument doc = new();
        doc.LoadHtml(html);

        var shortDescNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'shortdescription')]");
        if (shortDescNode != null)
        {
            string text = HtmlEntity.DeEntitize(shortDescNode.InnerText).Trim();
            return RegexPatterns.DisplayCleaner().Replace(text, "").Trim();
        }

        var pNode = doc.DocumentNode.SelectSingleNode("//p[b]");
        if (pNode != null)
        {
            string text = HtmlEntity.DeEntitize(pNode.InnerText).Trim();
            return RegexPatterns.DisplayCleaner().Replace(text, "").Trim();
        }

        return null;
    }

    public static string? GetRawFirstParagraph(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        HtmlDocument doc = new();
        doc.LoadHtml(html);

        var pNode = doc.DocumentNode.SelectSingleNode("//p[b]");
        pNode ??= doc.DocumentNode.SelectSingleNode(
            "//p[not(contains(@class,'mw-empty-elt')) and string-length(normalize-space()) > 20]"
        );

        if (pNode == null)
            return null;

        return HtmlEntity.DeEntitize(pNode.InnerText).Trim();
    }

    private static List<string> SplitAndCleanLines(string rawText) =>
        rawText.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
               .Select(l => l.Trim())
               .Where(l => !string.IsNullOrWhiteSpace(l))
               .ToList();

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
        int lastMeta = Math.Max(line.LastIndexOf(']'), line.LastIndexOf(')'));

        if (lastMeta >= 0 && lastMeta < line.Length - 1)
            return line[(lastMeta + 1)..].TrimDebris();

        int dash = line.IndexOfAny(['–', '-']);
        return dash >= 0
            ? line[(dash + 1)..].TrimDebris()
            : line.TrimDebris();
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
        var match = Regex.Match(description, @"(?<!\b[A-Z]|Mr|St|Dr)[.;]");

        return match.Success
            ? description[..match.Index].TrimDebris()
            : description.TrimDebris();
    }
}
