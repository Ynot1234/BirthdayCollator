using BirthdayCollator.Models;
using BirthdayCollator.Processing;
using BirthdayCollator.Services;
using Microsoft.AspNetCore.Mvc;

namespace BirthdayCollator.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BirthdaysController(BirthdayFetcher fetcher) : ControllerBase
{
    // ---------------------------------------------------------
    // PUSH MODEL (existing)
    // React sends month/day explicitly
    // ---------------------------------------------------------
    [HttpGet]
    public Task<List<Person>> Get(int month, int day) =>
        fetcher.GetBirthdays(month, day);


    // ---------------------------------------------------------
    // PULL MODEL #1
    // Server decides the date (today), applies override automatically
    // GET /api/birthdays/current
    // ---------------------------------------------------------
    [HttpGet("current")]
    public Task<List<Person>> GetCurrent()
    {
        var today = DateTime.Today;
        return fetcher.GetBirthdays(today.Month, today.Day);
    }


    // ---------------------------------------------------------
    // PULL MODEL #2
    // React pulls the current override from the server
    // GET /api/birthdays/override
    // ---------------------------------------------------------
    [HttpGet("override")]
    public IActionResult GetOverride([FromServices] IYearRangeProvider provider)
    {
        return Ok(new { overrideYear = provider.CurrentOverrideYear });
    }


    // ---------------------------------------------------------
    // PULL MODEL #3
    // React sets the override on the server
    // POST /api/birthdays/override?value=1933
    // ---------------------------------------------------------
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

    [ApiController]
    [Route("api/health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok(new { status = "ok" });
    }

}

