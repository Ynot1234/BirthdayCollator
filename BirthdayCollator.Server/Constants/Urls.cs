namespace BirthdayCollator.Constants;

public static class Urls
{

    // Base domain
    public const string Domain = "https://en.wikipedia.org";

    // Article pages
    public static readonly string ArticleBase = $"{Domain}/wiki";

    //API substring
    public const string APISub = $"/api/rest_v1/page";

    //// API HTML endpoint
    public static readonly string API = $"{Domain}/{APISub}/html";

    public const string GenarianBase = "https://www.genarians.com";

    public const string OnThisDayBase = "https://www.onthisday.com/birthdays";

    public const string APISearchStub = "w/api.php?action=query&list=search&srsearch=";

}
