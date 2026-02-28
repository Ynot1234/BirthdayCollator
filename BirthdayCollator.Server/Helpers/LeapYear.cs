namespace BirthdayCollator.Server.Helpers;

public static class LeapYear
{
    public static bool IsLeapFeb28(int month, int day)
    {
       return (DateTime.IsLeapYear(DateTime.Now.Year)) && (month == 2 && day == 28);
    }

}
