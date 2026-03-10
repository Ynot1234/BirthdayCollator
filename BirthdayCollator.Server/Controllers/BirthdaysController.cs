using BirthdayCollator.Server.Models;
using BirthdayCollator.Server.Processing.Builders;
using BirthdayCollator.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace BirthdayCollator.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BirthdaysController(
    BirthdayFetcher fetcher,
    IYearRangeProvider years) : ControllerBase
{
    [HttpGet]
    public async Task<List<Person>> Get(int month, int day, bool includeAll, CancellationToken ct)
    {
        return await fetcher.GetBirthdays(month, day, includeAll, ct);
    }

    [HttpDelete("{month:int}/{day:int}")]
    public IActionResult Clear(int month, int day)
    {
        fetcher.ClearCache(month, day);
        return NoContent();
    }

    [HttpGet("exists/{month:int}/{day:int}")]
    public bool Exists(int month, int day) => fetcher.IsCached(month, day);

    [HttpGet("years")]
    public ActionResult<IReadOnlyList<string>> GetYears() => Ok(years.GetDefaultYears());

    [HttpGet("current")]
    public Task<List<Person>> GetCurrent(CancellationToken ct) => Get(DateTime.Today.Month, DateTime.Today.Day, false, ct);

    [HttpGet("override")]
    public IActionResult GetOverride() =>  Ok(new { overrideYear = years.CurrentOverrideYear, includeAll = years.IncludeAll });

    [HttpPost("override")]
    public IActionResult SetOverride([FromBody] OverrideRequest req)
    {
        var result = req.Year switch
        {
            var y when string.IsNullOrWhiteSpace(y) => ClearAndReset(),
            var y when int.TryParse(y, out var parsed) => ApplyOverride(parsed, req.IncludeAll), _ => null
        };

        return result ?? BadRequest("Invalid year format.");

        IActionResult ClearAndReset()
        {
            years.ClearYear();
            years.SetIncludeAll(false);
            return GetOverride(); 
        }

        IActionResult ApplyOverride(int y, bool all)
        {
            years.ForceYear(y);
            years.SetIncludeAll(all);
            return GetOverride();
        }
    }
}