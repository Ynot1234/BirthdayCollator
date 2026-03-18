namespace BirthdayCollator.Server.Processing.Entries;

public record EntryContext(string RawText, string? Href, bool IsMulti, DateTime Date);

public interface IEntrySplitter
{
    bool IsMulti(string text);
    IReadOnlyList<string> Split(string text);
    bool IsDeathEntry(string text);
    IEnumerable<(string Text, DateTime Date)> SplitEntries(EntryContext ctx);
}

public sealed class EntrySplitter : IEntrySplitter
{
    public bool IsMulti(string text) =>
        !string.IsNullOrWhiteSpace(text) && text.AsSpan().ContainsAny('\n', '\r');

    public IReadOnlyList<string> Split(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        var span = text.AsSpan();
        var result = new List<string>();

        int lineCount = 0;
        foreach (var line in span.EnumerateLines())
        {
            var trimmed = line.Trim();
            if (trimmed.IsEmpty) continue;

            lineCount++;

            if (lineCount == 1) continue;

            result.Add(new string(trimmed));
        }

        if (lineCount == 1)
        {
            result.Add(text.Trim());
        }

        return result;
    }

    public bool IsDeathEntry(string entry) =>
        RegexPatterns.ExcludeDied().IsMatch(entry);

    public IEnumerable<(string Text, DateTime Date)> SplitEntries(EntryContext ctx)
    {
        foreach (var line in Split(ctx.RawText))
        {
            yield return (line, ctx.Date);
        }
    }
}