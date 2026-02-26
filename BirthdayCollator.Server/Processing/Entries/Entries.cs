namespace BirthdayCollator.Server.Processing.Entries;

public interface IEntrySplitter
{
    bool IsMulti(string rawText);
    IReadOnlyList<string> Split(string rawText);
    bool IsDeathEntry(string entry);
    IEnumerable<(string Text, DateTime Date)> SplitEntries(EntryContext ctx);
}

public sealed class EntrySplitter : IEntrySplitter
{
    public bool IsMulti(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return false;

        return rawText
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Length > 1;
    }

    public IReadOnlyList<string> Split(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return [];

        return [.. rawText
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)];
    }

    public bool IsDeathEntry(string entry)
    {
        return entry.Contains("(d.", StringComparison.OrdinalIgnoreCase)
            || entry.Contains("(died", StringComparison.OrdinalIgnoreCase);
    }

    public IEnumerable<(string Text, DateTime Date)> SplitEntries(EntryContext ctx)
    {
        if (!ctx.IsMulti)
            return [(ctx.RawText, ctx.Date)];

        return Split(ctx.RawText)
            .Select(t => (t, ctx.Date));
    }
}
