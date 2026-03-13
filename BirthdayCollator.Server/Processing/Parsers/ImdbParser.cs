using BirthdayCollator.Helpers;
using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using HtmlAgilityPack;
using static BirthdayCollator.Server.Constants.AppStrings;

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

            if (imgNode == null || string.IsNullOrEmpty(imgSrc) || imgSrc.Contains("nopicture") || imgSrc.Contains("default_main"))
            {
                continue;
            }

            var titleNode = node.SelectSingleNode(".//h3[contains(@class, 'ipc-title__text')]");
            var linkNode = node.SelectSingleNode(".//a[contains(@class, 'ipc-title-link-wrapper')]");

            if (titleNode != null && linkNode != null)
            {
                string name = RegexPatterns.BioLinkTail().Replace(HtmlEntity.DeEntitize(titleNode.InnerText), "").Trim();

                var bioNode = node.SelectSingleNode(".//div[contains(@class, 'ipc-html-content-inner-div')]")
                ?? node.SelectSingleNode(".//div[contains(@class, 'dli-bio')]");

                string description = bioNode != null ? HtmlEntity.DeEntitize(bioNode.InnerText).Trim(): "";

                if (description.Contains("was a") || description.Contains("was an")  || description.Contains("dies"))
                    continue;

                description = RegexPatterns.ExcludeBirthStatement().Replace(description, "").Trim();
                description = RegexPatterns.BioLinkSuffix().Replace(description, "");
                description = description.Replace("  ", " ").Trim();

                if (string.IsNullOrWhiteSpace(description))
                {
                    var metaNode = node.SelectSingleNode(".//div[contains(@class, 'dli-title-metadata')]");
                    description = metaNode?.InnerText.Trim() ?? String.Empty;
                }

                if (description.Length > 0 && char.IsLower(description[0]))
                {
                    description = char.ToUpper(description[0]) + description[1..];
                }

                description = RegexPatterns.ExcludeImdbFooter().Replace(description, "");

                string href = linkNode.GetAttributeValue("href", "");
                string slug = href.Split('/', StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1)?.Split('?')[0] ?? "";

                var p = personFactory.CreatePerson(
                    name: name,
                    desc: description,
                    birthDate: bDay,
                    url: $"{Urls.ImdbBase}/name/{slug}/",
                    sourceSlug: Slugs.Imdb,
                    section: String.Empty,
                    displaySlug: slug
                );

                results.Add(personFactory.Finalize(p));
            }
        }

        return results;
    }
}