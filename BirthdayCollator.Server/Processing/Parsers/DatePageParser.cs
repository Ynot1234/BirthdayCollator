namespace BirthdayCollator.Server.Processing.Parsers;

public sealed partial class DatePageParser(BirthEntryValidator validator,
                                            PersonFactory personFactory,
                                            ILinkResolver linkResolver) : IDatePageParser
{
    public List<Person> Parse(string html, int month, int day)
    {
        HtmlDocument htmlDoc = new();
        htmlDoc.LoadHtml(html);

        var liNodes = WikipediaDomNavigator.ExtractBirthLiNodes(htmlDoc);
        if (liNodes.Count is 0) return [];

        List<Person> results = [];

        string monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month);
        string sourceSlug = $"{monthName}_{day}";

        foreach (var li in liNodes)
        {
            string entry = WikiTextUtility.Normalize(li.InnerText);

            if (!entry.Contains('–') && !entry.Contains('-')) continue;

            if (!WikiTextUtility.TryExtractBirthYear(entry, out int birthYear)) continue;
            if (!validator.IsValidBirthEntry(entry, birthYear, month, day, li)) continue;

            var link = linkResolver.FindPersonLink(li, entry);
            if (link is null) continue;

            string name = !string.IsNullOrWhiteSpace(link.InnerText)
                ? WikiTextUtility.Normalize(link.InnerText)
                : WikiTextUtility.ExtractPersonName(entry);

            string description = WikiTextUtility.SanitizeWikiText(entry);

            string rawHref = link.GetAttributeValue("href", string.Empty);
            ReadOnlySpan<char> hrefSpan = rawHref.AsSpan();
            int lastSlash = hrefSpan.LastIndexOf('/');

            string slug = lastSlash == -1
                ? new string(hrefSpan.TrimStart('.'))
                : new string(hrefSpan[(lastSlash + 1)..].TrimStart('.'));

            results.Add(personFactory.CreatePerson(
                name: name,
                desc: description,
                birthDate: new DateTime(birthYear, month, day),
                url: !string.IsNullOrEmpty(slug) ? $"{Urls.ArticleBase}/{slug}" : string.Empty,
                sourceSlug: sourceSlug,
                section: Sections.Births,
                displaySlug: slug
            ));
        }
        return results;
    }
}