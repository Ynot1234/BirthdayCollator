using BirthdayCollator.Server.Processing.Builders;

public sealed class YearRangeProvider : IYearRangeProvider
{
    private readonly List<string> _defaults;
    private readonly List<string> _overrides = [];

    public string? CurrentOverrideYear => _overrides.Count == 1 ? _overrides[0] : null;
    public string CurrentOverrideSuffix { get; private set; } = string.Empty;
    public bool IncludeAll { get; private set; } = true;
    private static readonly int[] first = new[] { 60, 70, 80 };

    public YearRangeProvider()
    {
        int cur = DateTime.Now.Year;
        var ages = first.Concat(Enumerable.Range(85, 105 - 85 + 1));
       _defaults = [.. ages.Select(age => (cur - age).ToString()).OrderByDescending(y => y)];
    }

    public void ForceYear(int year) { _overrides.Clear(); _overrides.Add(year.ToString()); }
    public void ForceYears(params string[] years) { _overrides.Clear(); _overrides.AddRange(years); }
    public void ClearYear() => _overrides.Clear();
    public void SetIncludeAll(bool value) => IncludeAll = value;
    public void ForceSuffix(string s) => CurrentOverrideSuffix = s;
    public void ClearSuffix() => CurrentOverrideSuffix = string.Empty;

    public IReadOnlyList<string> GetYears() => _overrides.Count > 0 ? _overrides : _defaults;
    public IReadOnlyList<string> GetDefaultYears() => _defaults;

    public IReadOnlyList<string> GetLeapYears() =>
        [.. GetYears().Where(y => DateTime.IsLeapYear(int.Parse(y)))];
}
