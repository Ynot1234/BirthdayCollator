using BirthdayCollator.Helpers;

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
        !string.IsNullOrWhiteSpace(text) && text.Contains('\n');

    public IReadOnlyList<string> Split(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        // Splitting by newline and carriage return to handle different OS formats safely
        var lines = text
            .Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(l => l.Length > 0)
            .ToArray();

        // If it's a multi-line entry, we usually want everything after the header/first line
        return lines.Length > 1
            ? lines[1..]
            : lines;
    }

    public bool IsDeathEntry(string entry) =>
        RegexPatterns.ExcludeDied().IsMatch(entry);

    public IEnumerable<(string Text, DateTime Date)> SplitEntries(EntryContext ctx)
    {
        return Split(ctx.RawText).Select(t => (t, ctx.Date));
    }
}