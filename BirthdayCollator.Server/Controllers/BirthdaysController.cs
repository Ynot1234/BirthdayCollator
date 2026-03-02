using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace BirthdayCollator.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BirthdaysController : ControllerBase
{
    private readonly BirthdayFetcher fetcher;
    private readonly IMemoryCache cache;
    private readonly IYearRangeProvider yearRangeProvider;

    public BirthdaysController(
        BirthdayFetcher fetcher,
        IMemoryCache cache,
        IYearRangeProvider yearRangeProvider)
    {
        this.fetcher = fetcher;
        this.cache = cache;
        this.yearRangeProvider = yearRangeProvider;
    }

    // ---------------------------------------------------------
    // GET birthdays
    // ---------------------------------------------------------
    [HttpGet]
    public async Task<List<Person>> Get(int month, int day, bool? includeAll, CancellationToken token)
    {
        yearRangeProvider.SetIncludeAll(includeAll ?? false);
        return await fetcher.GetBirthdays(month, day, token);
    }

    // ---------------------------------------------------------
    // Clear cache
    // ---------------------------------------------------------
    [HttpDelete("{month:int}/{day:int}")]
    public void Clear(int month, int day)
    {
        string cacheKey = $"birthdays:{month}:{day}";
        cache.Remove(cacheKey);
    }

    // ---------------------------------------------------------
    // Cache exists?
    // ---------------------------------------------------------
    [HttpGet("exists/{month:int}/{day:int}")]
    public bool Exists(int month, int day)
    {
        string cacheKey = $"birthdays:{month}:{day}";
        return cache.TryGetValue(cacheKey, out _);
    }

    // ---------------------------------------------------------
    // ALWAYS return full year list (ignore overrides)
    // ---------------------------------------------------------
    [HttpGet("years")]
    public ActionResult<IReadOnlyList<string>> GetYears()
    {
        return Ok(yearRangeProvider.GetDefaultYears());
    }

    // ---------------------------------------------------------
    // Today's birthdays
    // ---------------------------------------------------------
    [HttpGet("current")]
    public async Task<List<Person>> GetCurrent(CancellationToken token)
    {
        var today = DateTime.Today;
        return await Get(today.Month, today.Day, false, token);
    }

    // ---------------------------------------------------------
    // Get override state
    // ---------------------------------------------------------
    [HttpGet("override")]
    public IActionResult GetOverride()
    {
        return Ok(new
        {
            overrideYear = yearRangeProvider.CurrentOverrideYear,
            includeAll = yearRangeProvider.IncludeAll
        });
    }

    // ---------------------------------------------------------
    // Set override state
    // ---------------------------------------------------------
    [HttpPost("override")]
    public IActionResult SetOverride([FromBody] OverrideRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Year))
        {
            yearRangeProvider.ClearYear();
            yearRangeProvider.SetIncludeAll(false);

            return Ok(new
            {
                overrideYear = (string?)null,
                includeAll = false
            });
        }

        yearRangeProvider.ForceYear(int.Parse(request.Year));
        yearRangeProvider.SetIncludeAll(request.IncludeAll);

        return Ok(new
        {
            overrideYear = request.Year,
            includeAll = request.IncludeAll
        });
    }
}
