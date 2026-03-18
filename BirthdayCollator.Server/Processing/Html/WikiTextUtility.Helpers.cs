using System.Buffers; 

namespace BirthdayCollator.Server.Processing.Html;

public static partial class WikiTextUtility
{
    private static readonly SearchValues<char> PrefixMetaChars = SearchValues.Create("])");
    private static readonly SearchValues<char> DashChars = SearchValues.Create("–-");

    private static List<string> SplitAndCleanLines(string rawText) =>
        [.. rawText.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
             .Select(l => l.Trim())
             .Where(l => !string.IsNullOrWhiteSpace(l))];

    private static string SelectTargetLine(List<string> lines, string fallback)
    {
        string? line = lines.FirstOrDefault(l => !MonthNames.All.Any(m =>
                 l.Equals(m, StringComparison.OrdinalIgnoreCase) ||
                (l.StartsWith(m, StringComparison.OrdinalIgnoreCase) && l.Length < m.Length + 5)
            ));

        return line ?? lines.FirstOrDefault() ?? fallback;
    }

    private static string ExtractCoreDescription(string line)
    {
        ReadOnlySpan<char> span = line.AsSpan();

        int prefixMeta = span.LastIndexOfAny(PrefixMetaChars);

        if (prefixMeta >= 0 && prefixMeta < 25)
            return new string(span[(prefixMeta + 1)..]).TrimDebris();

        int dash = span.IndexOfAny(DashChars);
        if (dash >= 0 && dash < 25)
            return new string(span[(dash + 1)..]).TrimDebris();

        return line.TrimDebris();
    }

    private static string RemovePersonName(string description, string? personName)
    {
        if (string.IsNullOrEmpty(personName))
            return description;

        return description.Replace(personName, "", StringComparison.OrdinalIgnoreCase)
                          .TrimDebris();
    }

    private static string RemoveTitles(string description)
    {
        foreach (var title in NameParsing.Titles)
        {
            if (description.Contains(title, StringComparison.OrdinalIgnoreCase))
            {
                description = description.Replace(title, "", StringComparison.OrdinalIgnoreCase);
            }
        }
        return description.TrimDebris();
    }

    private static string RemovePrefixes(string description)
    {
        ReadOnlySpan<char> descSpan = description.AsSpan();
        foreach (var prefix in NameParsing.Prefixes)
        {
            if (descSpan.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return new string(descSpan[prefix.Length..]).TrimDebris();
        }
        return description;
    }

    private static string StripLeadingVerbs(string description)
    {
        ReadOnlySpan<char> span = description.AsSpan();

        if (span.StartsWith("is ", StringComparison.OrdinalIgnoreCase) && span.Length > 3)
            return new string(span[3..]).TrimDebris();

        if (span.StartsWith("was ", StringComparison.OrdinalIgnoreCase) && span.Length > 4)
            return new string(span[4..]).TrimDebris();

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