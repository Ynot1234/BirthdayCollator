using System.Globalization;

namespace BirthdayCollator.Server.Constants;

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

    public const string APISearchStub = "/w/api.php?action=query&list=search&srsearch=";

    public const string DDGSearchBase = "https://duckduckgo.com";

    public const string OpenAIEmbeddings = "https://api.openai.com/v1/embeddings";

    public static string GetWikiBirthsUrl(string slug) =>
        $"{ArticleBase}/{slug}#{AppStrings.Sections.Births}";

    public static string GetOnThisDayUrl(int month, int day)
    {
        string monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month).ToLower();
        return $"{OnThisDayBase}/{monthName}/{day}";
    }

    public static string? GetSourceUrl(string? slug, string fallbackUrl) => slug switch
    {
        null => null,
        AppStrings.Slugs.OnThisDay => fallbackUrl,
        _ => GetWikiBirthsUrl(slug)
    };

}
