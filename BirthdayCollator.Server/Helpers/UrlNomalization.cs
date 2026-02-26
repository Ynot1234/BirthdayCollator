namespace BirthdayCollator.Server.Helpers;

public static class UrlNomalization
{

    public static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        url = url.Trim();

        if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            url = url["https://".Length..];

        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            url = url["http://".Length..];

        return url;
    }


    public static string NormalizeWikiUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        url = url.Trim().ToLowerInvariant();
        url = url.Replace("_,jr.", "_jr.");
        url = url.Replace("jr.", "jr"); // optional
        url = url.Replace("jr", "jr");  // no-op but keeps consistency
        url = url.Replace(",", "");

        return url;
    }
}