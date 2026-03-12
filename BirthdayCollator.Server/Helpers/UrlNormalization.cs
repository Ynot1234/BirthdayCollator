using BirthdayCollator.Server.Constants;

namespace BirthdayCollator.Server.Helpers;

public static class UrlNormalization
{
    public static string Fix(string? url, string name)
    {
        if (string.IsNullOrWhiteSpace(url) ||
            url.Equals("null", StringComparison.OrdinalIgnoreCase) ||
            url.Equals("undefined", StringComparison.OrdinalIgnoreCase))
        {
            return $"{Urls.DDGSearchBase}/?q={Uri.EscapeDataString(name)}";
        }

        string cleanUrl = url.StartsWith("http") ? url : $"https://{url}";

        const string suffix = "_(disambiguation)";
        if (cleanUrl.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            cleanUrl = cleanUrl[..^suffix.Length];
        }

        return cleanUrl;
    }

    public static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return string.Empty;
        url = url.Trim();
        if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return url[8..];
        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) return url[7..];
        return url;
    }
}