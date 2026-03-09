using BirthdayCollator.Server.Processing.Html;

namespace BirthdayCollator.Server.Processing.Links;

public static class WikiValidator
{
    public static bool IsDateHref(string href)
    {
        if (string.IsNullOrWhiteSpace(href)) return false;
        string trimmed = href.TrimStart('.', '/');

        // Check for year format (e.g., /1990)
        if (trimmed.Length == 4 && int.TryParse(trimmed, out _)) return true;

        // Check for date format (e.g., March_8)
        var parts = trimmed.Split('_');
        return parts.Length == 2 && DateTime.TryParse($"{parts[0]} 1", out _) && int.TryParse(parts[1], out _);
    }

    public static bool HrefMatchesName(string href, string name) =>
        WikiTextUtility.FuzzyNameMatch(href, name);
}
