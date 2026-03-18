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

        return final.Length < 3 ? targetLine.Trim() : final;
    }

    public static string? GetFirstBioParagraph(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return null;

        HtmlDocument doc = new();
        doc.LoadHtml(html);

        var shortDescNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'shortdescription')]");
        if (shortDescNode is not null)
        {
            return CleanNodeText(shortDescNode);
        }

        var pNode = doc.DocumentNode.SelectSingleNode("//p[b]");
        if (pNode is not null)
        {
            return CleanNodeText(pNode);
        }

        return null;
    }

    private static string CleanNodeText(HtmlNode node)
    {
        string text = HtmlEntity.DeEntitize(node.InnerText).Trim();
        return RegexPatterns.DisplayCleaner().Replace(text, "").Trim();
    }

    public static string? GetRawFirstParagraph(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return null;

        HtmlDocument doc = new();
        doc.LoadHtml(html);

        var pNode = doc.DocumentNode.SelectSingleNode("//p[b]")
                   ?? doc.DocumentNode.SelectSingleNode("//p[not(contains(@class,'mw-empty-elt')) and string-length(normalize-space()) > 20]");

        return pNode is null ? null : HtmlEntity.DeEntitize(pNode.InnerText).Trim();
    }

    public static string? ExtractSpecificParenthetical(string text, string personName)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        ReadOnlySpan<char> textSpan = text.AsSpan();
        string lastName = personName.Split(' ').Last();

        int lastIndex = text.LastIndexOf(lastName, StringComparison.OrdinalIgnoreCase);
        if (lastIndex == -1) return null;

        int nameEnd = lastIndex + lastName.Length;

        ReadOnlySpan<char> searchArea = textSpan[nameEnd..];
        int startRel = searchArea.IndexOf('(');

        if (startRel != -1 && startRel < 5)
        {
            int startAbs = nameEnd + startRel;
            int end = text.IndexOf(')', startAbs + 1);

            if (end != -1)
            {
                string candidate = text[startAbs..(end + 1)];

                if (RegexPatterns.YearIndicator().IsMatch(candidate))
                {
                    return candidate;
                }
            }
        }

        return null;
    }
}