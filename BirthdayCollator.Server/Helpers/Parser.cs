using BirthdayCollator.Constants;
using BirthdayCollator.Models;
using BirthdayCollator.Processing;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace BirthdayCollator.Helpers
{
    public partial class Parser(BirthEntryValidator validator, PersonFactory personFactory)
    {
        readonly bool AllforYear = false;  // If true, will include all entries from year page, ignoring date match. 

        public record EntryContext(string RawText, string? Href, bool IsMulti, DateTime Date);

        public List<Person> Parse(string html, DateTime actualDate, string? suffix, string xpath)
        {
            HtmlNode? birthsHeader = HtmlParsing.SelectNode(html, xpath);
            
            if (birthsHeader is null)
                return [];

            List<HtmlNode> sections = [.. GetBirthSections(birthsHeader)];
            List<HtmlNode> liNodes = GetLiNodes(sections);
           
            if (liNodes.Count == 0)
                return [];

            List<Person> results = [];

            foreach (HtmlNode li in liNodes)
            {

              if(li.InnerText.Contains("Reinsdorf"))
                {
                    int r = 4;

                }

                string raw = HtmlEntity.DeEntitize(li.InnerText).Trim();

                bool isMulti = IsMultiPersonEntry(raw);

                string dateHeaderRaw = raw;
                string? dateHeader = GetYearPageDateHeader(dateHeaderRaw);

               


                if (string.IsNullOrWhiteSpace(dateHeader))
                    continue; 

                if (!TryParseMonthDay(dateHeader, actualDate.Year, out DateTime parentDate))
                    continue;


                string? href = ExtractPersonHref(li.InnerHtml);

                EntryContext ctx = new(raw, href, isMulti, parentDate);

                if (!AllforYear && !MatchesRequestedDate(dateHeader, actualDate, ctx.IsMulti))
                    continue;
                var entries = GetEntries(ctx);

                foreach (var (Text, Date) in entries)
                {
                    string? entryHref = ExtractHrefForEntry(li, Text);

                    ProcessEntry(
                        results,
                        li,
                        Text,
                        ctx with { Href = entryHref },
                        Date,
                        suffix,
                        personFactory,
                        validator);
                }
            }

            return results;
        }


        private static string? ExtractHrefForEntry(HtmlNode li, string personName)
        {
            var anchors = li.SelectNodes(".//a");
            if (anchors == null || anchors.Count < 2)
                return null;

            // Skip the date anchor
            foreach (var a in anchors.Skip(1))
            {
                string text = HtmlEntity.DeEntitize(a.InnerText).Trim();

                if (personName.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                    text.Contains(personName, StringComparison.OrdinalIgnoreCase))
                {
                    return a.GetAttributeValue("href", "");
                }
            }

            return null;
        }



        public List<Person> ParseBirthsForDate(string html, int year, int month, int day)
        {
            var date = new DateTime(year, month, day);


            return Parse(
                html: html,
                actualDate: date,
                suffix: null,
                xpath:  XPathSelectors.GeneralYearHeader
            );
        }


        private static bool TryParseMonthDay(string text, int year, out DateTime result)
        {
            result = default;

            var match = RegexPatterns.MonthDayLooseRegex().Match(text);

            if (!match.Success)
                return false;

            string monthName = match.Groups[1].Value;
            string dayString = match.Groups[2].Value;

            return DateTime.TryParse($"{monthName} {dayString} {year}", out result);
        }

        public static void ProcessEntry(List<Person> results,
                                        HtmlNode li,
                                        string entry,
                                        EntryContext ctx,
                                        DateTime birthDate,
                                        string? suffix,
                                        PersonFactory personFactory,
                                        BirthEntryValidator validator)
        {
            if (IsDeathEntry(entry))
                return;

            if (!validator.IsValidBirthEntry(entry, birthDate.Year, li))
                return;

            var personLink = TryFindPersonLinkForEntry(li, entry);

            if (personLink is null)
                return;

            Person parsed = personFactory.BuildPerson(entry, birthDate, personLink);

            string idSuffix = suffix == String.Empty 
                ? $"{birthDate.Year}" 
                : $"{birthDate.Year}_{suffix}";

            Person person = personFactory.Create(parsed, idSuffix);

            FixSwappedName(person);

            if (!TryApplyHrefOverride(person, ctx.Href))
             return;
            

            results.Add(person);
        }


        private static IEnumerable<HtmlNode> GetBirthSections(HtmlNode birthsHeader)
        {
          
                int index = 0;

                for (HtmlNode node = birthsHeader.NextSibling; 
                     node != null; 
                     node = node.NextSibling,
                     index++)
                {
                    if (node.NodeType == HtmlNodeType.Text)
                    {
                        continue;
                    }

                    if (node.Name.Equals("h2", StringComparison.OrdinalIgnoreCase))
                    {
                        yield break;
                    }

                    HtmlNodeCollection lis = node.SelectNodes(".//li");

                    if (lis == null)
                    {
                        continue;
                    }

                    foreach (var li in lis)
                    {
                        yield return li;
                    }
                }

                yield break;
        }

        public static HtmlNode? TryFindPersonLinkForEntry(HtmlNode li, string entry)
        {
            var links = li.SelectNodes(".//a");
            if (links is null || links.Count == 0)
                return null;

            string personName = ExtractPersonName(entry);
            string normalizedPerson = HtmlEntity.DeEntitize(personName).Trim();

            List<HtmlNode> nonDateLinks = [.. links
                .Where(a =>
                {
                    string href = a.GetAttributeValue("href", "");
                    return !IsDateHref(href);
                })];

            if (nonDateLinks.Count == 0)
                return null;

            foreach (var a in nonDateLinks)
            {
                string linkText = HtmlEntity.DeEntitize(a.InnerText).Trim();
                if (string.Equals(linkText, normalizedPerson, StringComparison.OrdinalIgnoreCase))
                    return a;
            }

            foreach (var a in nonDateLinks)
            {
                string linkText = HtmlEntity.DeEntitize(a.InnerText).Trim();
                bool personContainsLink = normalizedPerson.Contains(linkText, StringComparison.OrdinalIgnoreCase);
                bool linkContainsPerson = linkText.Contains(normalizedPerson, StringComparison.OrdinalIgnoreCase);

                if (personContainsLink || linkContainsPerson)
                    return a;
            }

            return nonDateLinks.FirstOrDefault();
        }

        
        public static List<HtmlNode> GetLiNodes(IEnumerable<HtmlNode> sections)
        {
            List<HtmlNode> results = [];

            foreach (var section in sections)
            {
                if (section.Name.Equals("li", StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(section);
                    continue;
                }

                HtmlNodeCollection liNodes = section.SelectNodes(".//li");

                if (liNodes != null)
                {
                    results.AddRange(liNodes);
                    continue;
                }
            }

            return results;
        }


        public static bool IsDeathEntry(string entry)
        {
            return entry.Contains("(d.", StringComparison.OrdinalIgnoreCase)
                || entry.Contains("(died", StringComparison.OrdinalIgnoreCase);
        }

        public static bool MatchesRequestedDate(string text, DateTime date, bool isMultiPerson)
        {
            var match = RegexPatterns.MonthDayLooseRegex().Match(text);

            if (!match.Success)
                return false;

            string monthName = match.Groups[1].Value;
            string dayString = match.Groups[2].Value;

            if (!DateTime.TryParse($"{monthName} {dayString} {date.Year}", out DateTime parsedSingle))
                return false;

            return parsedSingle.Month == date.Month
                && parsedSingle.Day == date.Day;
        }

        
        public static bool TryApplyHrefOverride(Person person, string? href)
        {
            if (string.IsNullOrWhiteSpace(href))
                return true;

            string slug = WikiUrlBuilder.NormalizeWikiHref(href);
            person.Url = Urls.Domain + slug;

            return HrefMatchesName(href, person.Name);
        }

        public static IEnumerable<(string Text, DateTime Date)> GetEntries(EntryContext ctx)
        {
            if (!ctx.IsMulti)
                return [(ctx.RawText, ctx.Date)];

            return SplitYearPageEntry(ctx.RawText)
                  .Select(t => (t, ctx.Date));
        }

        public static string? GetYearPageDateHeader(string rawText)
        {
            var lines = rawText
                .Split('\n', StringSplitOptions.RemoveEmptyEntries);

            var first = lines.FirstOrDefault();
            if (first is null)
                return null;

            return first.Trim();
        }

        public static string? ExtractPersonHref(string innerHtml)
        {
            var wrapper = HtmlNode.CreateNode("<div>" + innerHtml + "</div>");
            var anchors = wrapper.SelectNodes(".//a");

            if (anchors is { Count: >= 2 })
            {
                var href = anchors[1].GetAttributeValue("href", "");
                return string.IsNullOrWhiteSpace(href) ? null : href;
            }

            return null;
        }


        public static string ExtractPersonName(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var idx = text.IndexOf('(');
            if (idx > 0)
                text = text[..idx];

            text = text.Trim().TrimEnd(',');

            return text;
        }

        public static void FixSwappedName(Person person)
        {
            if (person == null || string.IsNullOrWhiteSpace(person.Name))
                return;

            if (person.Name.Contains(','))
            {
                var parts = person.Name.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var first = parts[1].Trim();
                    var last = parts[0].Trim();
                    person.Name = $"{first} {last}";
                }
            }
        }

        public static bool IsDateHref(string href)
        {
            if (string.IsNullOrWhiteSpace(href)) return false;
            if (!href.StartsWith("./")) return false;

            string rest = href[2..];
            var parts = rest.Split('_');
            if (parts.Length != 2) return false;

            string month = parts[0];
            string day = parts[1];

            return IsMonthName(month) && int.TryParse(day, out _);
        }

        public static IEnumerable<string> SplitYearPageEntry(string rawText)
        {
            return rawText
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                .Select(l => l.Trim());
        }

        public static bool IsMultiPersonEntry(string rawText)
        {
            return rawText
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Length > 1;
        }

        public static bool IsMonthName(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return MonthNames.All.Contains(text, StringComparer.OrdinalIgnoreCase);
        }

        public static bool HrefMatchesName(string href, string name)
        {
            if (string.IsNullOrWhiteSpace(href) 
             || string.IsNullOrWhiteSpace(name))
                return false;

            // Normalize
            href = Normalize(href);
            name = Normalize(name);

            // Tokenize
            List<string> hrefTokens = Tokenize(href);
            List<string> nameTokens = Tokenize(name);

            // Remove stopwords
            hrefTokens = [.. hrefTokens.Except(Stopwords)];
            nameTokens = [.. nameTokens.Except(Stopwords)];

            // Titles to ignore
            hrefTokens = [.. hrefTokens.Except(Titles)];
            nameTokens = [.. nameTokens.Except(Titles)];

           // Every name token must appear in href tokens (partial match allowed)
            foreach (string nt in nameTokens)
            {
               bool tokenMatches = hrefTokens.Any(ht => ht.Contains(nt) || nt.Contains(ht));

                if (!tokenMatches)
                    return false;
            }

            return true;

        }

        private static string Normalize(string s)
        {
            s = s.ToLowerInvariant();
            s = s.Replace("_", " ");

            // Remove diacritics
            s = s.Normalize(NormalizationForm.FormD);
            var chars = s.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
            s = new string([.. chars]);

            return s;
        }

        private static List<string> Tokenize(string s)
        {
            return [.. s
                .Split([' ', ',', '.', '/', '-', '(', ')', '\''], StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => t.Length > 0)];
        }

        private static readonly HashSet<string> Stopwords = new(StringComparer.OrdinalIgnoreCase)
        {
            "of", "the", "and", "a", "an"
        };

        private static readonly HashSet<string> Titles = new(StringComparer.OrdinalIgnoreCase)
        {
            "sir", "dr", "mr", "mrs", "ms", "prof",
            "earl", "duke", "baron", "lord", "lady",
            "king", "queen", "prince", "princess",
            "count", "countess", "viscount", "marquess"
        };
    }
}