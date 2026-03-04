using BirthdayCollator.Server.Processing.Enrichment;
using Microsoft.AspNetCore.Mvc;

namespace BirthdayCollator.Server.Controllers;

public sealed class SummarizeRequest
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string? ApiKey { get; set; } // optional for dev
}

[ApiController]
[Route("api/ai")]
public class AIController : ControllerBase
{
    private readonly IPersonEnrichmentService enrichmentService;
    private readonly IConfiguration config;

    public AIController(IPersonEnrichmentService enrichmentService, IConfiguration config)
    {
        this.enrichmentService = enrichmentService;
        this.config = config;
    }

    // Reports whether a server-side key exists (dev only)
    [HttpGet("has-key")]
    public IActionResult HasKey()
    {
        var key = config["OpenAI:ApiKey"];
        bool hasKey = !string.IsNullOrWhiteSpace(key);
        return Ok(new { hasKey });
    }

    // Summarize using a user-supplied key (prod) or fallback to User Secrets (dev)
    [HttpPost("summarize")]
    public async Task<IActionResult> Summarize([FromBody] SummarizeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest("Name and description are required.");
        }

        // Pass the key along (may be null → dev fallback)
        string result = await enrichmentService.GetSummaryAsync(
            request.Name,
            request.Description,
            request.ApiKey
        );

        return Ok(result);
    }
}
