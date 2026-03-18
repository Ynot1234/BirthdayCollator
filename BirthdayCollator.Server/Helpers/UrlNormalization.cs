using System.Text.RegularExpressions;

namespace BirthdayCollator.Server.Helpers;

public static partial class UrlNormalization
{
  
    public static string Fix(string? url, string desc, string name)
    {
        if (string.IsNullOrWhiteSpace(url) ||
            url is "null" or "undefined" ||
            url.Contains("action=edit", StringComparison.OrdinalIgnoreCase))
        {
            return CreateDDGSearchUrl(name, desc);
        }

        string cleanUrl = url.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? url : $"https://{url}";

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

        cleanDesc = RegexPatterns.VerbPrefixRegex().Replace(cleanDesc, "").TrimDebris();

        string searchTerm = string.IsNullOrWhiteSpace(cleanDesc) ? name : $"{name} {cleanDesc}";

        return $"{Urls.DDGSearchBase}/?q={Uri.EscapeDataString(searchTerm)}";
    }

    public static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return string.Empty;

        ReadOnlySpan<char> urlSpan = url.AsSpan().Trim();

        if (urlSpan.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return new string(urlSpan[8..]);
        if (urlSpan.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) return new string(urlSpan[7..]);

        return new string(urlSpan);
    }
}