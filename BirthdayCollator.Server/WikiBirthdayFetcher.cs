using BirthdayCollator.Constants;
using BirthdayCollator.Models;
using BirthdayCollator.Processing;
using BirthdayCollator.Server.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BirthdayCollator.Server;

//public partial class WikiBirthdayFetcher(
//    IPersonPipeline pipeline,
//    IFetchPipeline fetchPipeline,
//    HttpClient http)
//{
//    private readonly BirthDateResolver _dateResolver = new();

//    public record BirthdayResult
//    {
//        public List<string> People { get; init; } = [];
//        public List<Person> PeopleObjects { get; init; } = [];
//        public string? Error { get; init; }
//    }

//    //public async Task<BirthdayResult> GetLivingPeopleBornOnDateAsync(
//    //    string month,
//    //    int day,
//    //    CancellationToken token)
//    //{
//    //    try
//    //    {
//    //        DateTime actualDate = _dateResolver.ResolveActualDate(month, day);

//    //        List<Person> allPeople = await fetchPipeline.FetchAllAsync(actualDate, token);
//    //        allPeople = await pipeline.Process(allPeople);
//    //        List<string> formatted = Format(allPeople);

//    //        return new BirthdayResult
//    //        {
//    //            People = formatted,
//    //            PeopleObjects = allPeople,
//    //            Error = null
//    //        };
//    //    }
//    //    catch (OperationCanceledException)
//    //    {
//    //        return new BirthdayResult
//    //        {
//    //            People = [],
//    //            PeopleObjects = [],
//    //            Error = "Canceled"
//    //        };
//    //    }
//    //    catch (Exception ex)
//    //    {
//    //        return new BirthdayResult
//    //        {
//    //            People = [],
//    //            PeopleObjects = [],
//    //            Error = ex.Message
//    //        };
//    //    }
//    //}


//    //public List<string> Format(List<Person> people)
//    //{
//    //    return [.. people.Select(p => $"{p.BirthYear} - {p.Name}, {p.Description}|{p.Url}")];
//    //}

//    //public async Task<WikiSummary?> GetSummaryAsync(string slug)
//    //{
//    //    try
//    //    {
//    //        var url = $"{Urls.Domain}/{Urls.APISub}/summary/{slug}";
//    //        var json = await http.GetStringAsync(url);
//    //        return JsonSerializer.Deserialize<WikiSummary>(json);
//    //    }
//    //    catch (Exception ex)
//    //    {
//    //        Console.WriteLine("ERROR: " + ex.Message);
//    //        return null;
//    //    }
//    //}


//}

