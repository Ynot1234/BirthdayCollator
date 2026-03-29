using System.Text.Json;
using System.Text.RegularExpressions;

namespace BirthdayCollator.Server.Processing.Parsers;

public sealed partial class ImdbParser(PersonFactory personFactory)
{
    public List<Person> Parse(string html, int month, int day, int startYear, int endYear)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(html);

        var lineNodes = doc.DocumentNode.SelectNodes("//td[@class='line-content']");
        string cleanCode = lineNodes != null
            ? HtmlEntity.DeEntitize(string.Join("\n", lineNodes.Select(n => n.InnerText)))
            : HtmlEntity.DeEntitize(html);

        var match = Regex.Match(cleanCode, @"<script[^>]*id=""__NEXT_DATA__""[^>]*>(.*?)</script>", RegexOptions.Singleline);

        if (match.Success)
        {
            return ParseFromJson(match.Groups[1].Value, month, day, startYear, endYear);
        }

        var nodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'ipc-metadata-list-item')]")
                    ?? doc.DocumentNode.SelectNodes("//li[contains(@class, 'ipc-metadata-list-item')]")
                    ?? doc.DocumentNode.SelectNodes("//div[contains(@class, 'dli-parent')]");

        return nodes != null ? ParseFromNodes(nodes, month, day, startYear, endYear) : [];
    }
    private List<Person> ParseFromJson(string jsonContent, int month, int day, int startYear, int endYear)
    {
        List<Person> results = [];
        using JsonDocument jdoc = JsonDocument.Parse(jsonContent);

        JsonElement? resultsArray = FindResultsArray(jdoc.RootElement);

        if (!resultsArray.HasValue || resultsArray.Value.ValueKind != JsonValueKind.Array)
            return results;

        foreach (var item in resultsArray.Value.EnumerateArray())
        {
            JsonElement data = item.TryGetProperty("node", out var node) ? node : item;

            // Simplified image check
            bool hasImage = (data.TryGetProperty("primaryImage", out var img) && img.ValueKind != JsonValueKind.Null) ||
                            (data.TryGetProperty("image", out var oldImg) && oldImg.ValueKind != JsonValueKind.Null);

            if (!hasImage) continue;

            string rawId = (data.TryGetProperty("id", out var id) ? id.GetString() : null)
                          ?? (data.TryGetProperty("nconst", out var nc) ? nc.GetString() : null)
                          ?? (data.TryGetProperty("nameId", out var ni) ? ni.GetString() : null)
                          ?? "";

            var slugMatch = RegexPatterns.PersonId().Match(rawId);

            if (!slugMatch.Success)
            {
                slugMatch = RegexPatterns.PersonId().Match(data.GetRawText());
            }

            if (!slugMatch.Success) continue;

            string slug = slugMatch.Value;
            string name = GetDeepString(data, "nameText") ?? GetDeepString(data, "name") ?? "Unknown";
            string bio = GetDeepString(data, "descriptionText") ?? GetDeepString(data, "bio") ?? "";
            string secondary = GetDeepString(data, "secondaryText") ?? "";

            string searchContext = $"{secondary} {bio}";

            string[] deathKeywords = ["died ", "died in", "passed away", "deceased", "death"];
            if (deathKeywords.Any(k => searchContext.Contains(k, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            int? birthYear = null;
            var yearMatches = RegexPatterns.YearCandidate().Matches(searchContext);
            foreach (Match m in yearMatches)
            {
                if (int.TryParse(m.Value, out int foundYear))
                {
                    if (foundYear >= startYear && foundYear <= endYear)
                    {
                        birthYear = foundYear;
                        break;
                    }
                }
            }

            string personUrl = $"{Urls.ImdbBase}/name/{slug}/";
            results.Add(CreatePersonObject(name, bio, birthYear, month, day, slug, null, personUrl));
        }

        return results;
    }
    private List<Person> ParseFromNodes(HtmlNodeCollection nodes, int month, int day, int startYear, int endYear)
    {
        List<Person> results = [];
        foreach (var node in nodes)
        {
            var titleNode = node.SelectSingleNode(".//h3") ?? node.SelectSingleNode(".//a");
            if (titleNode == null) continue;

            string name = RegexPatterns.LeadingRank().Replace(HtmlEntity.DeEntitize(titleNode.InnerText), "").Trim();

            var linkNode = node.SelectSingleNode(".//a");
            string href = linkNode?.GetAttributeValue("href", "") ?? "";
            var slugMatch = RegexPatterns.PersonId().Match(href);
            string slug = slugMatch.Success ? slugMatch.Value : "no-slug-" + Guid.NewGuid().ToString()[..5];

            var imgNode = node.SelectSingleNode(".//img");
            if (imgNode == null) continue;

            int? birthYear = null;
            string allText = node.InnerText;

            string[] deathKeywords = ["died ", "died in", "passed away", "deceased", "death"];
            if (deathKeywords.Any(k => allText.Contains(k, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var yearMatches = RegexPatterns.YearCandidate().Matches(allText);

            foreach (Match m in yearMatches)
            {
                if (int.TryParse(m.Value, out int foundYear))
                {
                    if (foundYear >= startYear && foundYear <= endYear)
                    {
                        birthYear = foundYear;
                        break;
                    }
                }
            }

            string personUrl = $"{Urls.ImdbBase}/name/{slug}/";
            results.Add(CreatePersonObject(name, "Scraped Node", birthYear, month, day, slug, node, personUrl));
        }
        return results;
    }
    #region Helpers

    private static JsonElement? FindResultsArray(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in element.EnumerateObject())
            {
                if ((prop.Name == "results" || prop.Name == "nameListItems") && prop.Value.ValueKind == JsonValueKind.Array)
                    return prop.Value;

                var found = FindResultsArray(prop.Value);
                if (found != null) return found;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var found = FindResultsArray(item);
                if (found != null) return found;
            }
        }
        return null;
    }

    private static string? GetDeepString(JsonElement item, string key)
    {
        if (!item.TryGetProperty(key, out var el)) return null;
        if (el.ValueKind == JsonValueKind.String) return el.GetString();
        if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty("text", out var t))
            return t.GetString();

        return null;
    }

    private Person CreatePersonObject(string name, string desc, int? y, int m, int d, string slug, HtmlNode? node, string personUrl)
    {
        int yearToUse = y ?? 1; // Null becomes 1, used as a flag later

        var p = personFactory.CreatePerson(
            name: name,
            desc: node != null ? CleanDescription(node, desc, name) : desc,
            birthDate: new DateTime(yearToUse, m, d),
            url: personUrl,
            sourceSlug: Slugs.Imdb,
            section: string.Empty,
            displaySlug: slug
        );

        return p;
    }

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
        if (knownForIdx != -1) return desc[(knownForIdx + "known for".Length)..].Trim();

        foreach (var prefix in NameParsing.Prefixes)
        {
            if (desc.AsSpan().StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return desc[prefix.Length..].Trim();
        }
        return desc;
    }

    #endregion
}