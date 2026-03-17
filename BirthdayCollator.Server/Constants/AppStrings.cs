namespace BirthdayCollator.Server.Constants;

public static class AppStrings
{
    public static class Sections
    {
        public const string Births = "Births";
    }

    public static class Slugs
    {
        public const string Genarians = "Genarians";
        public const string OnThisDay = "OnThisDay";
        public const string Wikipedia = "Wikipedia";
        public const string Birthdays = "Birthdays";
        public const string Imdb = "Imdb";
    }

    public static class DateFormats
    {
        public const string FullDate = "MM/dd/yyyy";
        public const string MonthLong = "MMMM";
    }

    public static class HttpClients
    {
        public const string Wikipedia = "WikiClient";
        public const string Genarians = "GenarianClient";
    }
}