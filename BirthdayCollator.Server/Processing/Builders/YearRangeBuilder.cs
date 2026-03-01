namespace BirthdayCollator.Server.Processing.Builders;

public class YearRangeBuilder()
{
    public List<string> BuildYearRange()
    {
        int currentYear = DateTime.Now.Year;
        
        List<int> years =
        [
            currentYear - 60,
            currentYear - 70,
            currentYear - 80
        ];

        for (int age = 85; age <= 105; age++)
        {
            years.Add(currentYear - age);
        }

        return [.. years.OrderByDescending(y => y)
                        .Select(y => y.ToString())];
    }


    public List<string> BuildLeapYearRange()
    {
        return [.. BuildYearRange()
            .Select(int.Parse)
            .Where(DateTime.IsLeapYear)
            .Select(y => y.ToString())];
    }


}
