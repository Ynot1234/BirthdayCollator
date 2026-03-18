namespace BirthdayCollator.Server.Processing.Html;

public static partial class WikiTextUtility
{
    public static string NormalizeDescription(string text)
    {
        text = Normalize(text);
        if (string.IsNullOrWhiteSpace(text)) return text;

        foreach (var prefix in NameParsing.Prefixes)
        {
            if (text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                text = text[prefix.Length..].TrimStart();
                break;
            }
        }

        return text.Length > 0 ? char.ToUpper(text[0]) + text[1..] : text;
    }
}