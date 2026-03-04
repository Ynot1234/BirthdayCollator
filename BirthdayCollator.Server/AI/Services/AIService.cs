using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BirthdayCollator.Server.AI.Services;

public interface IAIService
{
    Task<string> SummarizeAsync(string name, string desc, string? apiKey = null);
}

public class AIService(IConfiguration config) : IAIService
{
    public async Task<string> SummarizeAsync(string name, string desc, string? apiKey = null)
    {
        // 1. Prefer user-supplied key (production)
        var key = apiKey;

        // 2. Fall back to server key (dev via User Secrets)
        if (string.IsNullOrWhiteSpace(key))
        {
            key = config["OpenAI:ApiKey"];
        }

        // 3. If still no key → return safe message (prevents HTML fallback)
        if (string.IsNullOrWhiteSpace(key))
        {
            return "No OpenAI API key provided.";
        }

        // Build the kernel on demand using the resolved key
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("gpt-4o-mini", key);

        var kernel = builder.Build();

        var systemPrompt = @"
            You are a professional summarization assistant.
            Your only job is to summarize the provided text.
            Do not greet the user.
            Do not introduce yourself.
            Do not add emojis.
            Only return the summary.";

        IChatCompletionService chat = kernel.GetRequiredService<IChatCompletionService>();

        ChatHistory messages = new();
        messages.AddSystemMessage(systemPrompt);
        messages.AddUserMessage(name + desc);

        ChatMessageContent result = await chat.GetChatMessageContentAsync(messages);
        return result.ToString();
    }
}
