using BirthdayCollator.Helpers;

namespace BirthdayCollator.Server.Processing.Entries;

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
        !string.IsNullOrWhiteSpace(text) && text.Contains('\n');

    public IReadOnlyList<string> Split(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        var lines = text
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToArray();

        return lines.Length > 1
            ? lines[1..]  
            : lines;      
    }

    public bool IsDeathEntry(string entry) =>
        RegexPatterns.ExcludeDiedRegex().IsMatch(entry);

    public IEnumerable<(string Text, DateTime Date)> SplitEntries(EntryContext ctx)
    {
        return Split(ctx.RawText).Select(t => (t, ctx.Date));
    }
}
