namespace BirthdayCollator.Server.Processing.Builders;

public sealed class YearRangeProvider : IYearRangeProvider
{
    public string? CurrentOverrideYear { get; private set; }

    private readonly List<string> _forcedYears = [];
    public IReadOnlyList<string> ForcedYears => _forcedYears;

    public string CurrentOverrideSuffix { get; private set; } = string.Empty;

    private readonly YearRangeBuilder _builder;
    private readonly List<string> _defaultYears;

    public YearRangeProvider()
    {
        _builder = new YearRangeBuilder();
        _defaultYears = _builder.BuildYearRange();
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

    public IReadOnlyList<string> GetLeapYears()
    {
        // If a single override year is set, respect it.
        if (CurrentOverrideYear is not null)
        {
            return DateTime.IsLeapYear(int.Parse(CurrentOverrideYear))
                ? [CurrentOverrideYear]
                : [];
        }

        // If forced years exist, filter them.
        if (_forcedYears.Count > 0)
        {
            return [.. _forcedYears
                .Select(int.Parse)
                .Where(DateTime.IsLeapYear)
                .Select(y => y.ToString())];
        }

        // Otherwise, use the builder's leap-year range.
        return _builder.BuildLeapYearRange();
    }
}
