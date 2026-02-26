using BirthdayCollator.Models;
using BirthdayCollator.Helpers;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using BirthdayCollator.Server.Constants;



namespace BirthdayCollator.Server.Processing.Pipelines;

public partial class PersonFilter(IHttpClientFactory factory)
{
    private readonly HttpClient _http = factory.CreateClient("WikiClient");

    public List<Person> FilterLivingPeople(List<Person> people)
    {
        var living = new List<Person>();

        foreach (var person in people)
        {
            if (person.Description.Contains("died", StringComparison.OrdinalIgnoreCase))
                continue;

            if (!IsPersonDead(person.Url, person.BirthDate))
                living.Add(person);
        }


        return living;
    }

    private bool IsPersonDead(string url, DateTime birthdate)
    {
        string html;

        try
        {
            html = _http.GetStringAsync(url).GetAwaiter().GetResult();
        }
        catch
        {
            return false; // assume alive if page can't load
        }

        string? paren = ExtractParen(html);

        if (paren == null)
            return false; // no info → assume alive

        // Rule 1: If dash exists → dead
        if (paren.Contains('–'))
            return true;

        // Rule 2: No dash → must match birthdate to be alive
        bool matches = FirstParenDateMatches(paren, birthdate);

        // If it doesn't match → treat as dead/unreliable
        return !matches;
    }


    private static bool FirstParenDateMatches(string paren, DateTime birthdate)
    {
        var match = RegexPatterns.LongFormDateRegex().Match(paren);
       
        if (!match.Success)
            return false;

        string dateText = match.Value;

        if (!DateTime.TryParse(dateText, out DateTime parsed))
            return false;

        return parsed.Month == birthdate.Month &&
               parsed.Day == birthdate.Day;
    }



    private static string? ExtractParen(string html)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(html);

        var pNodes = doc.DocumentNode.SelectNodes("//p");
        if (pNodes == null)
            return null;

        foreach (var p in pNodes)
        {
            string text = p.InnerText;

            int start = 0;

            while ((start = text.IndexOf('(', start)) >= 0)
            {
                int end = text.IndexOf(')', start + 1);

                if (end < 0)
                    break;

                string paren = text.Substring(start, end - start + 1);

                if (ContainsMonth(paren))
                    return paren;

                if (paren.Contains("d.", StringComparison.OrdinalIgnoreCase) ||
                    paren.Contains("died", StringComparison.OrdinalIgnoreCase))
                    return paren;

                start = end + 1;
            }
        }

        return null;
    }

    private static bool ContainsMonth(string s)
    {
        return MonthNames.All.Any(m => s.Contains(m, StringComparison.OrdinalIgnoreCase));
    }
}