namespace BirthdayCollator.Server.Processing.Builders;

public interface IYearRangeProvider
{
    IReadOnlyList<string> GetYears();
    IReadOnlyList<string> GetLeapYears();
    IReadOnlyList<string> GetDefaultYears();

    IReadOnlyList<string> GetGenarianYears(); 

    void ForceYears(params string[] years);
    void ForceYear(int year);
    void ClearYear();
    string? CurrentOverrideYear { get; }

    void ForceSuffix(string suffix);
    void ClearSuffix();
    string CurrentOverrideSuffix { get; }

    bool IncludeAll { get; }
    void SetIncludeAll(bool value);
}
