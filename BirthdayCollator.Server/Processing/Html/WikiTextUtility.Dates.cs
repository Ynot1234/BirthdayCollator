using System.Buffers;

namespace BirthdayCollator.Server.Processing.Html;

public static partial class WikiTextUtility
{
    private static readonly SearchValues<string> DeathTriggers =
        SearchValues.Create([" died ", " death ", " deceased ", " disappeared "], StringComparison.OrdinalIgnoreCase);

    public static bool TryExtractBirthYear(string normalizedText, out int birthYear)
    {
        birthYear = 0;
        ReadOnlySpan<char> span = normalizedText.AsSpan();

        int dashIndex = span.IndexOfAny('–', '-');

        if (dashIndex == -1) return false;

        ReadOnlySpan<char> preDash = span[..dashIndex].Trim();

        int lastSpace = preDash.LastIndexOf(' ');
        ReadOnlySpan<char> potentialYear = lastSpace == -1 ? preDash : preDash[(lastSpace + 1)..];

        return int.TryParse(potentialYear, out birthYear);
    }

    public static bool IndicatesDeath(string? paren, DateTime birthDate)
    {
        if (string.IsNullOrWhiteSpace(paren)) return false;

        ReadOnlySpan<char> parenSpan = paren.AsSpan();

        var match = RegexPatterns.DeathYearMarker().Match(paren);
        if (match.Success)
        {
            var yearGroup = match.Groups["deathYear"].Success
                            ? match.Groups["deathYear"]
                            : match.Groups["diedYear"];

            if (int.TryParse(yearGroup.ValueSpan, out int foundYear))
            {
                return foundYear > birthDate.Year;
            }
        }

        return parenSpan.ContainsAny(DeathTriggers);
    }
}