using BirthdayCollator.Server.Constants;
using System.Text.RegularExpressions;

namespace BirthdayCollator.Server.Helpers;

public static class UrlNormalization
{
    public static string Fix(string? url, string desc, string name)
    {
        if (string.IsNullOrWhiteSpace(url) ||
        url.Equals("null", StringComparison.OrdinalIgnoreCase) ||
        url.Equals("undefined", StringComparison.OrdinalIgnoreCase) ||
        url.Contains("action=edit"))
        {
            return CreateDDGSearchUrl(name, desc);
        }

        string cleanUrl = url.StartsWith("http") ? url : $"https://{url}";

        const string suffix = "_(disambiguation)";
        if (cleanUrl.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            cleanUrl = cleanUrl[..^suffix.Length];
        }

        return cleanUrl;
    }


    public static string CreateDDGSearchUrl(string name, string desc)
    {
        string cleanDesc = Regex.Replace(desc ?? "", Regex.Escape(name), "", RegexOptions.IgnoreCase).TrimDebris();

        if (cleanDesc.StartsWith("is ", StringComparison.OrdinalIgnoreCase))
            cleanDesc = cleanDesc[3..].TrimDebris();
        else if (cleanDesc.StartsWith("was ", StringComparison.OrdinalIgnoreCase))
            cleanDesc = cleanDesc[4..].TrimDebris();

        string searchTerm = string.IsNullOrWhiteSpace(cleanDesc) ? name : $"{name} {cleanDesc}";

        return $"{Urls.DDGSearchBase}/?q={Uri.EscapeDataString(searchTerm)}";
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