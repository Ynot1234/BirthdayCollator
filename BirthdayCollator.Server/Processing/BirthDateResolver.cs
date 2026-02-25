using System;

namespace BirthdayCollator.Processing;

public class BirthDateResolver()
{
    public DateTime ResolveActualDate(string? month, int? day)
    {
        if (!string.IsNullOrEmpty(month) && day.HasValue)
        {
            string composite = $"{month} {day.Value}";

            if (DateTime.TryParse(composite, out DateTime parsed))
                return parsed;
        }

        return DateTime.Today;
    }
}
