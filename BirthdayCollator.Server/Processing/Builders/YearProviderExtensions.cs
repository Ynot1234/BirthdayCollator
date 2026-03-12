using BirthdayCollator.Server.Models;

namespace BirthdayCollator.Server.Processing.Builders;

public static class YearProviderExtensions
{
    public static void HandleOverride(this IYearRangeProvider provider, OverrideRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Year))
        {
            provider.ClearYear();
            provider.SetIncludeAll(false);
        }
        else if (int.TryParse(req.Year, out var parsed))
        {
            provider.ForceYear(parsed);
            provider.SetIncludeAll(req.IncludeAll);
        }
        else throw new ArgumentException("Invalid year format.");
    }
}