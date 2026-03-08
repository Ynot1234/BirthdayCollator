using BirthdayCollator.Server.Processing.Enrichment;
using Microsoft.AspNetCore.Mvc;

namespace BirthdayCollator.Server.Controllers;

[ApiController]
[Route("api/ai")]
public class AIController(IPersonEnrichmentService enrichment, IConfiguration config) : ControllerBase
{
    [HttpGet("has-key")]
    public IActionResult HasKey() => Ok(new { hasKey = !string.IsNullOrWhiteSpace(config["OpenAI:ApiKey"]) });

    [HttpPost("summarize")]
    public async Task<IActionResult> Summarize([FromBody] SummarizeRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Description))
            return BadRequest("Name and description are required.");

        var serverKey = config["OpenAI:ApiKey"]; // from User Secrets
        var userKey = Request.Headers["X-OpenAI-Key"].FirstOrDefault();

        string? keyToUse = !string.IsNullOrWhiteSpace(serverKey)
            ? serverKey
            : userKey;

        string result = await enrichment.GetSummaryAsync(req.Name, req.Description, keyToUse);

        return result.Contains("No API key") ? BadRequest(result) : Ok(result);
    }
}

public record SummarizeRequest(string Name, string Description);
