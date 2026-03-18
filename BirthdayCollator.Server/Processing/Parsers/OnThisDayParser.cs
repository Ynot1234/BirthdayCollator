namespace BirthdayCollator.Server.Processing.Parsers;

public sealed class OnThisDayParser(PersonFactory personFactory)
{
    public List<Person> Parse(string html, int month, int day)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(html);

        var nodes = doc.DocumentNode.SelectNodes("//li[contains(@class, 'person')]");
        if (nodes is null) return [];

        List<Person> results = [];

        foreach (var li in nodes)
        {
            string raw = HtmlEntity.DeEntitize(li.InnerText).Trim();

            string? yearText = li.SelectSingleNode(".//a[contains(@class, 'birthDate')]")?.InnerText
                               ?? li.SelectSingleNode(".//b")?.InnerText;

            if (!TryExtractYear(yearText, out int year))
                continue;

            if (raw.Contains("(d.", StringComparison.OrdinalIgnoreCase))
                continue;

            string yearStr = year.ToString();
            string trimmed = raw.StartsWith(yearStr) ? raw[yearStr.Length..].Trim() : raw;

            var (name, desc) = SplitNameAndDescription(trimmed);

            if (string.IsNullOrWhiteSpace(name))
                continue;

            results.Add(personFactory.CreatePerson(
                name: name,
                desc: desc,
                birthDate: new DateTime(year, month, day),
                url: string.Empty,
                sourceSlug: Slugs.OnThisDay,
                section: Sections.Births,
                displaySlug: name.Replace(' ', '_') 
            ));
        }

        return results;
    }

    private static bool TryExtractYear(string? text, out int year)
    {
        year = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;

        Span<char> buffer = stackalloc char[10];
        int count = 0;
        foreach (char c in text)
        {
            if (char.IsDigit(c) && count < buffer.Length)
                buffer[count++] = c;
        }

        return int.TryParse(buffer[..count], out year);
    }

    private static (string name, string? description) SplitNameAndDescription(string text)
    {
        ReadOnlySpan<char> span = text.AsSpan().TrimStart([' ', ',', '-', '–']);

        int commaIndex = span.IndexOf(',');
        if (commaIndex < 0) return (new string(span).Trim(), null);

        return (
            new string(span[..commaIndex]).Trim(),
            new string(span[(commaIndex + 1)..]).Trim()
        );
    }
}