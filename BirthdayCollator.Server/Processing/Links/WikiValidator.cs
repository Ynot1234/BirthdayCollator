namespace BirthdayCollator.Server.Processing.Links;

public static class WikiValidator
{
    public static bool IsDateHref(string href)
    {
        if (string.IsNullOrWhiteSpace(href)) return false;

        var span = href.AsSpan().TrimStart("./");

        if (span.Length == 4 && int.TryParse(span, out _)) return true;

        int underscoreIndex = span.IndexOf('_');
        if (underscoreIndex == -1) return false;

        var part1 = span[..underscoreIndex];
        var part2 = span[(underscoreIndex + 1)..];

        return (underscoreIndex + 1 < span.Length) &&
               DateTime.TryParse(string.Concat(part1, " 1"), out _) &&
               int.TryParse(part2, out _);
    }

    public static bool HrefMatchesName(string href, string name) => WikiTextUtility.FuzzyNameMatch(href, name);
}
