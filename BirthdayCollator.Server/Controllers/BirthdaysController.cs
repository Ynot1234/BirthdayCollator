using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory; 

namespace BirthdayCollator.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BirthdaysController(BirthdayFetcher fetcher, IMemoryCache cache) : ControllerBase
{
    [HttpGet]
    public async Task<List<Person>> Get(int month, int day, CancellationToken token)
    {

        return await fetcher.GetBirthdays(month, day, token);

        //string cacheKey = $"birthdays:{month}:{day}";

        //return await cache.GetOrCreateAsync(cacheKey, async entry =>
        //{
        //    entry.Priority = CacheItemPriority.NeverRemove;
        //    return await fetcher.GetBirthdays(month, day, token);
        //}) ?? [];
    }

    [HttpDelete("{month:int}/{day:int}")]
    public void Clear(int month, int day)
    {
        string cacheKey = $"birthdays:{month}:{day}";
        cache.Remove(cacheKey);
    }

    [HttpGet("exists/{month:int}/{day:int}")]
    public bool Exists(int month, int day)
    {
        string cacheKey = $"birthdays:{month}:{day}";
        return cache.TryGetValue(cacheKey, out _);
    }


    [HttpGet("years")]
    public ActionResult<IReadOnlyList<string>> GetYears([FromServices] IYearRangeProvider provider)
    {
        return Ok(provider.GetYears());
    }



    // PULL MODEL #1
    [HttpGet("current")]
    public async Task<List<Person>> GetCurrent(CancellationToken token)
    {
        var today = DateTime.Today;
        return await Get(today.Month, today.Day, token);
    }

    // PULL MODEL #2
    [HttpGet("override")]
    public IActionResult GetOverride([FromServices] IYearRangeProvider provider)
    {
        return Ok(new { overrideYear = provider.CurrentOverrideYear });
    }

    // PULL MODEL #3
    [HttpPost("override")]
    public IActionResult SetOverride([FromQuery] string? value, [FromServices] IYearRangeProvider provider)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            provider.ClearYear();
            return Ok(new { overrideYear = (string?)null });
        }

        if (int.TryParse(value, out int year))
        {
            provider.ForceYear(year);
            return Ok(new { overrideYear = year.ToString() });
        }

        return BadRequest("Invalid year");
    }
}
