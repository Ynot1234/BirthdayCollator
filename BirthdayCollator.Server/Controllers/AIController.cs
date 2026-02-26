using BirthdayCollator.Server.AI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace BirthdayCollator.Controllers;

public sealed class SummarizeRequest
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}

[ApiController]
[Route("api/ai")]
public class AIController : ControllerBase
{
    private readonly IAIService _ai;
    private readonly IConfiguration _config;

    public AIController(IAIService ai, IConfiguration config)
    {
        _ai = ai;
        _config = config;
    }

    // ---------------------------------------------------------
    // NEW: Check if an OpenAI API key is configured
    // ---------------------------------------------------------
    [HttpGet("has-key")]
    public IActionResult HasKey()
    {
        var key = _config["OpenAI:ApiKey"];
        bool hasKey = !string.IsNullOrWhiteSpace(key);
        return Ok(new { hasKey });
    }

    // ---------------------------------------------------------
    // Existing: Generate a summary using your AI service
    // ---------------------------------------------------------
    [HttpPost("summarize")]
    public async Task<IActionResult> Summarize([FromBody] SummarizeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("The 'name' field is required.");

        if (string.IsNullOrWhiteSpace(request.Description))
            return BadRequest("The 'description' field is required.");

        string result = await _ai.SummarizeAsync(request.Name, request.Description);
        return Ok(result);
    }
}
