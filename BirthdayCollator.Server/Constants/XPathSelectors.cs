namespace BirthdayCollator.Server.Constants;
public static class XPathSelectors
{
    public const string YearBirthsHeader =  "//h2[@id='Births']";
    public const string CategoryBirthsHeader = "//span[@id='Births']/parent::h2 | //h2[@id='Births']";
    public const string DescendantAnchorHref = ".//a[@href]";
}