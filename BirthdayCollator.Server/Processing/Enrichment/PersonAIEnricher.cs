using BirthdayCollator.Server.Constants;

namespace BirthdayCollator.Server.Processing.Enrichment;

public sealed class PersonAIEnricher()
{
    public static string NormalizeDescription(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        foreach (var prefix in NameParsing.Prefixes)
        {
            if (text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                text = text[prefix.Length..].TrimStart();
                break;
            }
        }

        if (text.Length > 0)
            text = char.ToUpper(text[0]) + text.Substring(1);

        return text;
    }
}