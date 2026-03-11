namespace BirthdayCollator.Server.Processing.Builders;

public class WikiUrlBuilder()
{
    public static string NormalizeWikiHref(string href)
    {
        if (string.IsNullOrWhiteSpace(href))
            return href;

        if (href.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return href;

        if (href.StartsWith("./"))
            return string.Concat("/wiki/", href.AsSpan(2));

        if (href.StartsWith("/wiki/"))
            return href;

        return "/wiki/" + href.TrimStart('/');
    }
}