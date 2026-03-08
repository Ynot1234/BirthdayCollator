using BirthdayCollator.Server.Constants;

namespace BirthdayCollator.Server.Processing.Html;

public static partial class WikiTextUtility
{
    public static bool HasKeywordOverlap(string? a, string? b)
    {
        var wa = ExtractKeywords(a);
        var wb = ExtractKeywords(b);
        return wa.Intersect(wb).Count() >= 2;
    }

    private static HashSet<string> ExtractKeywords(string? text) =>
        string.IsNullOrWhiteSpace(text) ? [] :
        [.. text.ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(w => w.Trim(',', '.', ';', ':', '!', '?', '(', ')', '[', ']'))
            .Where(w => w.Length >= 3 && !NameParsing.Stopwords.Contains(w))];
}
