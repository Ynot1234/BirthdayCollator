namespace BirthdayCollator.Server.Processing.Html;

public static partial class WikiTextUtility
{
    public static string ExtractPersonName(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        ReadOnlySpan<char> span = text.AsSpan();
        int idx = span.IndexOf('(');

        ReadOnlySpan<char> namePart = idx > 0 ? span[..idx] : span;

        return new string(namePart.Trim().TrimEnd(','));
    }

    public static bool FuzzyNameMatch(string a, string b)
    {
        var aTokens = GetCleanTokens(a);
        var bTokens = GetCleanTokens(b);

        if (aTokens.Count == 0 || bTokens.Count == 0) return false;

        foreach (var at in aTokens)
        {
            foreach (var bt in bTokens)
            {
                if (at.Contains(bt, StringComparison.OrdinalIgnoreCase) ||
                    bt.Contains(at, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static HashSet<string> GetCleanTokens(string s)
    {
        var slug = StringNormalization.ToComparableSlug(s);
        if (string.IsNullOrWhiteSpace(slug)) return [];

        char[] separators = [' ', ',', '.', '/', '-', '(', ')', '[', ']', '\'', ':', ';'];

        var tokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var segment in slug.Split(separators, StringSplitOptions.RemoveEmptyEntries))
        {
            string trimmed = segment.Trim();

            if (trimmed.Length > 0 &&
                !NameParsing.Stopwords.Contains(trimmed) &&
                !NameParsing.Titles.Contains(trimmed))
            {
                tokens.Add(trimmed);
            }
        }

        return tokens;
    }
}