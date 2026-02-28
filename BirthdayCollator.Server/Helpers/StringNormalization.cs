namespace BirthdayCollator.Server.Helpers;

public static class StringNormalization
{

    public static string CleanName(string name)
    {
        return name.Trim().TrimEnd(',');
    }

    public static string RemoveParenthetical(string text)
    {
        int idx = text.IndexOf('(');
        return idx > 0 ? text[..idx].Trim() : text.Trim();
    }

    public static (string name, string? description) SplitNameAndDescription(string text)
    {
        int commaIndex = text.IndexOf(',');
        if (commaIndex < 0)
            return (text.Trim(), null);

        return (
            text[..commaIndex].Trim(),
            text[(commaIndex + 1)..].Trim()
        );
    }


}
