namespace BirthdayCollator.Server.Processing.Entries;

public record EntryContext(string RawText, string? Href, bool IsMulti, DateTime Date);
