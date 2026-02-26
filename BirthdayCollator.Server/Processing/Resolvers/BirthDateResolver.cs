using System;

namespace BirthdayCollator.Server.Processing.Resolvers;

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
