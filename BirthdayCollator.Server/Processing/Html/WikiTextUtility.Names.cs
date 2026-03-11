using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Helpers;

namespace BirthdayCollator.Server.Processing.Html;

public static partial class WikiTextUtility
{
    public static string ExtractPersonName(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        int idx = text.IndexOf('(');
        if (idx > 0) text = text[..idx];
        return text.Trim().TrimEnd(',');
    }

   public static bool FuzzyNameMatch(string a, string b)
    {
        var aTokens = GetCleanTokens(a);
        var bTokens = GetCleanTokens(b);

        return aTokens.Any(at => bTokens.Any(bt =>
            at.Contains(bt, StringComparison.OrdinalIgnoreCase) ||
            bt.Contains(at, StringComparison.OrdinalIgnoreCase)));
    }

    private static List<string> GetCleanTokens(string s)
    {
        var slug = StringNormalization.ToComparableSlug(s);

        List<string> tokens = [.. (slug
                                .Split([' ', ',', '.', '/', '-', '(', ')', '[', ']', '\'', ':', ';'],
                                        StringSplitOptions.RemoveEmptyEntries)
                                .Select(t => t.Trim())
                                .Where(t => t.Length > 0))
                              ];


        return [.. tokens
            .Except(NameParsing.Stopwords, StringComparer.OrdinalIgnoreCase)
            .Except(NameParsing.Titles, StringComparer.OrdinalIgnoreCase)
        ];
    }
}