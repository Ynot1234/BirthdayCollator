using System.Collections.Generic;

namespace BirthdayCollator.Processing;

public interface IYearRangeProvider
{
    IReadOnlyList<string> GetYears();
    IReadOnlySet<string> GetYearSet();

    // Existing multi-year override
    void ForceYears(params string[] years);
    void ResetToDefault();

    // NEW: single-year override (pull model)
    void ForceYear(int year);
    void ClearYear();

    string? CurrentOverrideYear { get; }
    IReadOnlyList<string> ForcedYears { get; }

    string CurrentOverrideSuffix { get; }

    void ForceSuffix(string suffix);
    void ClearSuffix();
}
