namespace BirthdayCollator.Server.Constants;

public static class XPathSelectors
{
    // Year pages (table-based)
    public const string YearBirthsHeader =
        "//h2[@id='Births']";

    // Category pages (h2 + span-based)
    public const string CategoryBirthsHeader =
        "//span[@id='Births']/parent::h2 | //h2[@id='Births']";

    public const string GeneralYearHeader =
        "//span[@id='Births']/parent::h2 | //h2[@id='Births']";

    public const string DescendantAnchorHref = ".//a[@href]";

}
