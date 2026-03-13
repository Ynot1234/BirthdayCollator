namespace BirthdayCollator.Server.Constants;

public static class Urls
{
    public const string Domain = "https://en.wikipedia.org";

    public const string ImdbBase = "https://www.imdb.com";

    public const string ImdbSearchStub = $"{ImdbBase}/search/name/?birth_date";

    public const string APISub = $"/api/rest_v1/page";
    
    public const string GenarianBase = "https://www.genarians.com";
    
    public const string OnThisDayBase = "https://www.onthisday.com/birthdays";
    
    public const string APISearchStub = "/w/api.php?action=query&list=search&srsearch=";
    
    public const string DDGSearchBase = "https://duckduckgo.com";
    
    public static readonly string ArticleBase = $"{Domain}/wiki";
    
    public static readonly string API = $"{Domain}/{APISub}/html";
    public static string GetWikiBirthsUrl(string slug) => $"{ArticleBase}/{slug}#{AppStrings.Sections.Births}";

    public static string? GetSourceUrl(string? slug, string fallbackUrl) => slug switch
    {
        null => null,
        AppStrings.Slugs.OnThisDay => fallbackUrl,
        AppStrings.Slugs.Imdb => fallbackUrl,
        _ => GetWikiBirthsUrl(slug)
    };

    // public const string OpenAIEmbeddings = "https://api.openai.com/v1/embeddings";

}
