namespace BirthdayCollator.Server.Processing.Parsers;
public sealed partial class ImdbParser(PersonFactory personFactory)
{
    public List<Person> Parse(string html, int year, int month, int day)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(html);

        var listItems = doc.DocumentNode.SelectNodes("//li[contains(@class, 'ipc-metadata-list-summary-item')]");
        if (listItems == null) return [];

        List<Person> results = [];
        DateTime bDay = new(year, month, day);

        foreach (var node in listItems)
        {
            var imgNode = node.SelectSingleNode(".//img[contains(@class, 'ipc-image')]");
            string imgSrc = imgNode?.GetAttributeValue("src", "") ?? "";

            if (string.IsNullOrEmpty(imgSrc) || imgSrc.Contains("nopicture") || imgSrc.Contains("default_main"))
                continue;

            var titleNode = node.SelectSingleNode(".//h3[contains(@class, 'ipc-title__text')]");
            var linkNode = node.SelectSingleNode(".//a[contains(@class, 'ipc-title-link-wrapper')]");

            if (titleNode is null || linkNode is null) continue;

            string name = RegexPatterns.LeadingRank().Replace(HtmlEntity.DeEntitize(titleNode.InnerText), "").Trim();

            var bioNode = node.SelectSingleNode(".//div[contains(@class, 'ipc-html-content-inner-div')]")
                ?? node.SelectSingleNode(".//div[contains(@class, 'dli-bio')]");

            string description = bioNode != null ? HtmlEntity.DeEntitize(bioNode.InnerText).Trim() : "";

            if (IsDeceased(description)) continue;

            string href = linkNode.GetAttributeValue("href", "");
            string slug = href.Split('/', StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1)?.Split('?')[0] ?? "";

            Person p = personFactory.CreatePerson(
                name: name,
                desc: CleanDescription(node, description, name),
                birthDate: bDay,
                url: $"{Urls.ImdbBase}/name/{slug}/",
                sourceSlug: Slugs.Imdb,
                section: string.Empty,
                displaySlug: slug
            );

            results.Add(personFactory.Finalize(p));
        }

        return results;
    }

    private static bool IsDeceased(string desc) =>
        desc.Contains("was a", StringComparison.OrdinalIgnoreCase) ||
        desc.Contains("was an", StringComparison.OrdinalIgnoreCase) ||
        desc.Contains("dies", StringComparison.OrdinalIgnoreCase) ||
        desc.Contains("died", StringComparison.OrdinalIgnoreCase);

    private static string CleanDescription(HtmlNode node, string description, string name)
    {
        if (description.StartsWith(name, StringComparison.OrdinalIgnoreCase))
            description = description[name.Length..].Trim();

        description = RegexPatterns.LeadingBirthStatement().Replace(description, "");
        description = RegexPatterns.ExcludeBirthStatement().Replace(description, "");
        description = RegexPatterns.BioLinkSuffix().Replace(description, "");
        description = RegexPatterns.ExcludeImdbFooter().Replace(description, "");
        description = RegexPatterns.LeadingConnectors().Replace(description, "");

        description = PrefixProcessing(description.Trim());

        description = RegexPatterns.LeadingConnectors().Replace(description, "").Trim();
        description = RegexPatterns.CollapseWhitespace().Replace(description, " ").Trim();

        if (string.IsNullOrWhiteSpace(description))
        {
            var metaNode = node.SelectSingleNode(".//div[contains(@class, 'dli-title-metadata')]");
            description = metaNode?.InnerText.Trim() ?? string.Empty;
        }

        return description.Length > 0 && char.IsLower(description[0])
        ? $"{char.ToUpper(description[0])}{description.AsSpan(1)}"
        : description;
    }

    private static string PrefixProcessing(string desc)
    {
        int knownForIdx = desc.IndexOf("known for", StringComparison.OrdinalIgnoreCase);

        if (knownForIdx != -1)
            return desc[(knownForIdx + "known for".Length)..].Trim();

        foreach (var prefix in NameParsing.Prefixes)
        {
            if (desc.AsSpan().StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return desc[prefix.Length..].Trim();
        }

        return desc;
    }
}