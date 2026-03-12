using BirthdayCollator.Helpers;
using BirthdayCollator.Server.Constants;
using HtmlAgilityPack;
using System.Text.RegularExpressions;


namespace BirthdayCollator.Server.Processing.Html;

public static partial class WikiTextUtility
{
    public static string ExtractDescription(string rawText, string? personName = null)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return string.Empty;

        var lines = rawText.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
                           .Select(l => l.Trim())
                           .Where(l => !string.IsNullOrWhiteSpace(l))
                           .ToList();

        string targetLine = lines.FirstOrDefault(
                            l => !MonthNames.All.Any(m => l.Equals(m, StringComparison.OrdinalIgnoreCase) ||
                            (l.StartsWith(m, StringComparison.OrdinalIgnoreCase) && l.Length < m.Length + 5)))
                            ?? lines.FirstOrDefault()
                            ?? rawText;

        int lastMetadataIndex = Math.Max(targetLine.LastIndexOf(']'), targetLine.LastIndexOf(')'));
        string description;

        if (lastMetadataIndex >= 0 && lastMetadataIndex < targetLine.Length - 1)
        {
            description = targetLine[(lastMetadataIndex + 1)..].Trim();
        }
        else
        {
            int dash = targetLine.IndexOfAny(['–', '-']);
            description = dash >= 0 ? targetLine[(dash + 1)..].Trim() : targetLine.Trim();
        }

        if (!string.IsNullOrEmpty(personName))
        {
            description = Regex.Replace(description, Regex.Escape(personName),  "", RegexOptions.IgnoreCase).Trim();
        }

        foreach (var title in NameParsing.Titles)
        {
            if (description.Contains(title, StringComparison.OrdinalIgnoreCase))
            {
                description = description.Replace(title, "", StringComparison.OrdinalIgnoreCase).Trim();
            }
        }

        description = description.TrimStart(' ', ',', '.', ';', ':', '-');

        foreach (var prefix in NameParsing.Prefixes)
        {
            if (description.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                description = description[prefix.Length..].Trim();
                break;
            }
        }

        if (description.StartsWith("is ", StringComparison.OrdinalIgnoreCase) && description.Length > 3)
            description = description[3..].Trim();
        else if (description.StartsWith("was ", StringComparison.OrdinalIgnoreCase) && description.Length > 4)
            description = description[4..].Trim();

        int sentenceEnd = description.IndexOfAny(['.', ';']);
        string final = sentenceEnd > 0 ? description[..sentenceEnd].Trim() : description.Trim();

        return string.IsNullOrWhiteSpace(final) || final.Length < 3 ? targetLine.Trim() : final;
    }
    
    public static string? GetFirstBioParagraph(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return null;

        HtmlDocument doc = new();
        doc.LoadHtml(html);

        var shortDescNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'shortdescription')]");
        if (shortDescNode != null)
        {
            string text = HtmlEntity.DeEntitize(shortDescNode.InnerText).Trim();
            return RegexPatterns.DisplayCleaner().Replace(text, "").Trim();
        }

        var pNode = doc.DocumentNode.SelectSingleNode(
            "//div[contains(@class,'mw-parser-output')]/p[not(contains(@class,'mw-empty-elt')) and normalize-space()]"
        );

        return pNode != null ? HtmlEntity.DeEntitize(pNode.InnerText).Trim() : null;
    }

    public static string? GetRawFirstParagraph(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return null;
        HtmlDocument doc = new();
        doc.LoadHtml(html);
        var pNode = doc.DocumentNode.SelectSingleNode("//p[b]");
        pNode ??= doc.DocumentNode.SelectSingleNode("//p[not(contains(@class,'mw-empty-elt')) and string-length(normalize-space()) > 20]");

        if (pNode == null) return null;

        return HtmlEntity.DeEntitize(pNode.InnerText).Trim();
    }
}