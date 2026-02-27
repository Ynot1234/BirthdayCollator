using BirthdayCollator.Server.Processing.Enrichment; 
using Microsoft.AspNetCore.Mvc;

namespace BirthdayCollator.Server.Controllers;

public sealed class SummarizeRequest
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}

[ApiController]
[Route("api/ai")]
public class AIController(IPersonEnrichmentService enrichmentService, IConfiguration config) : ControllerBase
{
    [HttpGet("has-key")]
    public IActionResult HasKey()
    {
        var key = config["OpenAI:ApiKey"];
        bool hasKey = !string.IsNullOrWhiteSpace(key);
        return Ok(new { hasKey });
    }

    [HttpPost("summarize")]
    public async Task<IActionResult> Summarize([FromBody] SummarizeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) 
         || string.IsNullOrWhiteSpace(request.Description))
            return BadRequest("Name and description are required.");

        string result = await enrichmentService.GetSummaryAsync(request.Name, request.Description);

        return Ok(result);
    }
}
