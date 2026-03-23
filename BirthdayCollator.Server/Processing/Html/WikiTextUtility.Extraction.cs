namespace BirthdayCollator.Server.Processing.Html;

public static partial class WikiTextUtility
{
    public static bool DebugEnabled { get; set; } = false;

    private static void Log(string message)
    {
        if (DebugEnabled)
            Console.WriteLine(message);
    }

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
            return CleanNodeText(shortDescNode);

        var pNode = doc.DocumentNode.SelectSingleNode("//p[b]");
        if (pNode is not null)
            return CleanNodeText(pNode);

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

        var pNode =
            doc.DocumentNode.SelectSingleNode("//p[b]") ??
            doc.DocumentNode.SelectSingleNode("//p[not(contains(@class,'mw-empty-elt')) and string-length(normalize-space()) > 20]");

        return pNode is null ? null : HtmlEntity.DeEntitize(pNode.InnerText).Trim();
    }

    public static string? ExtractSpecificParenthetical(string text, string personName)
    {
        Log("RAW TEXT:");
        Log(text);
        Log("");

        Log("RAW NAME:");
        Log(personName);
        Log("");

        string normalizedText = NormalizeApostrophes(text);
        string normalizedName = NormalizeApostrophes(personName);

        Log("NORMALIZED TEXT:");
        Log(normalizedText);
        Log("");

        Log("NORMALIZED NAME:");
        Log(normalizedName);
        Log("");

        string[] nameParts = normalizedName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string firstName = nameParts.FirstOrDefault() ?? string.Empty;
        string lastName = nameParts.LastOrDefault() ?? string.Empty;

        Log($"FIRST NAME: [{firstName}]");
        Log($"LAST NAME: [{lastName}]");

        int nameIndex = -1;
        int nameEnd = -1;

        int fullIndex = normalizedText.IndexOf(normalizedName, StringComparison.OrdinalIgnoreCase);
        Log($"FULL NAME INDEX: {fullIndex}");

        if (fullIndex != -1)
        {
            nameIndex = fullIndex;
            nameEnd = fullIndex + normalizedName.Length;
            Log("USING: full name match");
        }
        else
        {
            int firstIndex = -1;
            if (!string.IsNullOrEmpty(firstName))
                firstIndex = normalizedText.IndexOf(firstName, StringComparison.OrdinalIgnoreCase);

            Log($"FIRST NAME INDEX: {firstIndex}");

            if (firstIndex != -1 && !string.IsNullOrEmpty(lastName))
            {
                int lastIndexAfterFirst =
                    normalizedText.IndexOf(lastName, firstIndex + firstName.Length, StringComparison.OrdinalIgnoreCase);

                Log($"LAST NAME AFTER FIRST INDEX: {lastIndexAfterFirst}");

                if (lastIndexAfterFirst != -1)
                {
                    nameIndex = firstIndex;
                    nameEnd = lastIndexAfterFirst + lastName.Length;
                    Log("USING: first + last pattern match");
                }
            }

            if (nameIndex == -1 && !string.IsNullOrEmpty(lastName))
            {
                int lastOnlyIndex = normalizedText.IndexOf(lastName, StringComparison.OrdinalIgnoreCase);
                Log($"FALLBACK LAST NAME INDEX: {lastOnlyIndex}");

                if (lastOnlyIndex != -1)
                {
                    nameIndex = lastOnlyIndex;
                    nameEnd = lastOnlyIndex + lastName.Length;
                    Log("USING: last name only match");
                }
            }
        }

        if (nameIndex == -1 || nameEnd == -1)
        {
            Log("FAILURE: Could not anchor name in text.");
            return null;
        }

        Log($"FINAL NAME INDEX: {nameIndex}");
        Log($"FINAL NAME END: {nameEnd}");

        int parenStart = normalizedText.IndexOf('(', nameEnd);
        Log($"PAREN START: {parenStart}");

        if (parenStart == -1)
        {
            Log("FAILURE: '(' not found after name.");
            return null;
        }

        int distance = parenStart - nameEnd;
        Log($"DISTANCE FROM NAME TO '(': {distance}");

        const int MaxDistance = 80;

        if (distance > MaxDistance)
        {
            Log("FAILURE: '(' too far from name.");
            return null;
        }

        int parenEnd = normalizedText.IndexOf(')', parenStart + 1);
        Log($"PAREN END: {parenEnd}");

        if (parenEnd == -1)
        {
            Log("FAILURE: ')' not found.");
            return null;
        }

        string candidate = normalizedText.Substring(parenStart, parenEnd - parenStart + 1);
        Log($"CANDIDATE: {candidate}");

        bool matches = RegexPatterns.YearIndicator().IsMatch(candidate);
        Log($"REGEX MATCH: {matches}");

        if (matches)
        {
            Log("SUCCESS: Returning candidate.");
            return candidate;
        }

        Log("FAILURE: Regex did not match.");
        return null;
    }

    private static string NormalizeApostrophes(string s)
    {
        if (s == null) return "";

        return s
            .Replace('\u2019', '\'')
            .Replace('\u2018', '\'')
            .Replace('\u201B', '\'')
            .Replace('\u02BC', '\'')
            .Replace('\u02BB', '\'')
            .Replace('\u02BD', '\'')
            .Replace('\u02B9', '\'')
            .Replace('\u2032', '\'')
            .Replace('\u2035', '\'')
            .Replace('\uFF07', '\'')
            .Replace('\u275C', '\'')
            .Replace('\u275B', '\'')
            .Replace('\uA78C', '\'')
            .Replace('\uA78B', '\'')
            .Replace('\u05F3', '\'')
            .Replace('\u05F4', '\'');
    }
}
