using BirthdayCollator.Helpers;
using HtmlAgilityPack;
using BirthdayCollator.Server.Constants;
using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Entries;



namespace BirthdayCollator.Server.Processing.Pipelines;

public partial class PersonFilter(IHttpClientFactory factory, IEntrySplitter entrySplitter)
{
    private readonly HttpClient _http = factory.CreateClient("WikiClient");
    private readonly IEntrySplitter _entrySplitter = entrySplitter;

    public List<Person> FilterLivingPeople(List<Person> people)
    {
        List<Person> living = [];

        foreach (Person p in people)
        {
            if (p.Description.Contains("died", StringComparison.OrdinalIgnoreCase))
                continue;

            if (!IsPersonDead(p.Url, p.BirthDate))
                living.Add(p);
        }


        return living;
    }

    private bool IsPersonDead(string url, DateTime birthdate)
    {
        string html;

        try
        {
            html = _http.GetStringAsync(url).Result;
        }
        catch
        {
            return false;
        }


        string? paren = ExtractParen(html);

        if (paren == null)
            return false; 

        if (paren.Contains('–'))
            return true;

        bool matches = FirstParenDateMatches(paren, birthdate);

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

        return (parsed.Month == birthdate.Month && parsed.Day == birthdate.Day);
    }



    private string? ExtractParen(string html)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(html);

        var pNodes = doc.DocumentNode.SelectNodes("//p");
        
        if (pNodes == null)
            return null;

        foreach (HtmlNode p in pNodes)
        {
            string text = p.InnerText;

            int start = 0;

            while ((start = text.IndexOf('(', start)) >= 0)
            {
                int end = text.IndexOf(')', start + 1);

                if (end < 0)
                    break;

                string paren = text.Substring(start, end - start + 1);

                if (ContainsMonth(paren) || _entrySplitter.IsDeathEntry(paren)) 
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