namespace BirthdayCollator.Server.Helpers;

public static class LeapYear
{
    public static bool IsNonLeapFeb28(int month, int day)
    {
        return month == 2 && day == 28 && !DateTime.IsLeapYear(DateTime.Now.Year);
    }


}
