namespace BirthdayCollator.Server.Processing.Builders;

public sealed class YearRangeProvider : IYearRangeProvider
{
    // ---------------------------------------------------------
    // Internal state
    // ---------------------------------------------------------

    // Single-year override (pull model)
    public string? CurrentOverrideYear { get; private set; }

    // Multi-year override (existing behavior)
    private readonly List<string> _forcedYears = new();
    public IReadOnlyList<string> ForcedYears => _forcedYears;

    // Suffix override
    public string CurrentOverrideSuffix { get; private set; } = string.Empty;

    // Default year range (Blazor behavior)
    private readonly List<string> _defaultYears;


    // ---------------------------------------------------------
    // Constructor (RESTORES BLAZOR BEHAVIOR)
    // ---------------------------------------------------------
    public YearRangeProvider()
    {
        var builder = new YearRangeBuilder();
        _defaultYears = builder.BuildYearRange();
    }


    // ---------------------------------------------------------
    // YEAR OVERRIDES
    // ---------------------------------------------------------

    public void ForceYear(int year)
    {
        CurrentOverrideYear = year.ToString();
        _forcedYears.Clear();
    }

    public void ClearYear()
    {
        CurrentOverrideYear = null;
    }

    public void ForceYears(params string[] years)
    {
        _forcedYears.Clear();
        _forcedYears.AddRange(years);
        CurrentOverrideYear = null;
    }

    public void ResetToDefault()
    {
        _forcedYears.Clear();
        CurrentOverrideYear = null;
    }


    // ---------------------------------------------------------
    // SUFFIX OVERRIDES
    // ---------------------------------------------------------

    public void ForceSuffix(string suffix)
    {
        CurrentOverrideSuffix = suffix;
    }

    public void ClearSuffix()
    {
        CurrentOverrideSuffix = string.Empty;
    }


    // ---------------------------------------------------------
    // YEAR LIST ACCESSORS
    // ---------------------------------------------------------

    public IReadOnlyList<string> GetYears()
    {
        if (CurrentOverrideYear is not null)
            return [CurrentOverrideYear];

        if (_forcedYears.Count > 0)
            return _forcedYears;

        // RESTORED: use your custom Blazor year range
        return _defaultYears;
    }

    public IReadOnlySet<string> GetYearSet()
    {
        return GetYears().ToHashSet();
    }
}