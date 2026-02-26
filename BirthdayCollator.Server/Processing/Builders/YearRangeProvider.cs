namespace BirthdayCollator.Server.Processing.Builders;

public sealed class YearRangeProvider : IYearRangeProvider
{
    public string? CurrentOverrideYear { get; private set; }

    private readonly List<string> _forcedYears = new();
    public IReadOnlyList<string> ForcedYears => _forcedYears;

    public string CurrentOverrideSuffix { get; private set; } = string.Empty;

    private readonly List<string> _defaultYears;


    public YearRangeProvider()
    {
        var builder = new YearRangeBuilder();
        _defaultYears = builder.BuildYearRange();
    }


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


    public void ForceSuffix(string suffix)
    {
        CurrentOverrideSuffix = suffix;
    }

    public void ClearSuffix()
    {
        CurrentOverrideSuffix = string.Empty;
    }


    public IReadOnlyList<string> GetYears()
    {
        if (CurrentOverrideYear is not null)
            return [CurrentOverrideYear];

        if (_forcedYears.Count > 0)
            return _forcedYears;

        return _defaultYears;
    }

    public IReadOnlySet<string> GetYearSet()
    {
        return GetYears().ToHashSet();
    }
}