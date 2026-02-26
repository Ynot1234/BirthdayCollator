using BirthdayCollator.Server.AI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BirthdayCollator.Server.Controllers;

public sealed class SummarizeRequest
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
}

[ApiController]
[Route("api/ai")]
public class AIController(IAIService ai, IConfiguration config) : ControllerBase
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
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("The 'name' field is required.");

        if (string.IsNullOrWhiteSpace(request.Description))
            return BadRequest("The 'description' field is required.");

        string result = await ai.SummarizeAsync(request.Name, request.Description);
        return Ok(result);
    }
}
