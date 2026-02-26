using BirthdayCollator.Constants;
using BirthdayCollator.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace BirthdayCollator.Server.Processing.Parsers
{
    public sealed class OnThisDayParser
    {
        public List<Person> Parse(string html, int month, int day)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var liNodes = htmlDoc.DocumentNode.SelectNodes("//li[@class='person']");
            if (liNodes == null || liNodes.Count == 0)
                return [];

            var results = new List<Person>();

            foreach (var li in liNodes)
            {
                string raw = HtmlEntity.DeEntitize(li.InnerText).Trim();

                if (raw.Contains("(d."))
                    continue;

                if (!TryExtractYear(li, out int year))
                    continue;

                if (!TryExtractNameAndDescription(raw, year, out string name, out string description))
                    continue;

                string monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month).ToLowerInvariant();

                results.Add(new Person
                {
                    BirthYear = year,
                    Name = name,
                    Description = description,
                    Url = "",

                    Month = month,
                    Day = day,
                    Section = "Births",

                    SourceSlug = "onthisday",
                    DisplaySlug = "OnThisDay",
                    SourceUrl = $"{Urls.OnThisDayBase}/{monthName}/{day}"
                });


            }

            return results;
        }

        private static bool TryExtractYear(HtmlNode li, out int year)
        {
            year = 0;

            HtmlNode a = li.SelectSingleNode(".//a[@class='birthDate']");

            if (a != null && int.TryParse(a.InnerText.Trim(), out year))
                return true;

            HtmlNode b = li.SelectSingleNode(".//b");

            if (b != null && int.TryParse(b.InnerText.Trim(), out year))
                return true;

            return false;
        }

        private static bool TryExtractNameAndDescription(string raw, int year, out string name, out string description)
        {
            name = string.Empty;
            description = string.Empty;

            string trimmed = raw.StartsWith(year.ToString())
                ? raw[year.ToString().Length..].Trim()
                : raw;

            int commaIndex = trimmed.IndexOf(',');

            if (commaIndex < 0)
                return false;

            name = trimmed[..commaIndex].Trim();
            description = trimmed[(commaIndex + 1)..].Trim();

            return true;
        }
    }

}