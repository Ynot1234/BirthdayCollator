using BirthdayCollator.Server.AI.Utilities;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BirthdayCollator.Server.AI.Services;

public class AIService(IConfiguration config, IKernelFactory kernelFactory) : IBioService
{
    private const string SystemPrompt = "You are a professional biographical summarizer. Return only the summary text. No greetings, introductions, or emojis.";

    public async Task<string> SummarizeAsync(string name, string desc, string? apiKey = null)
    {
        var key = apiKey ?? config["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(key)) return "No OpenAI API key provided.";

        string fullText = $"{name} {desc}".Trim();
        var chunks = TextChunker.Chunk(fullText, 4000).ToList();
        var tasks = chunks.Select(c => CallAiAsync(c, key));
        var summaries = await Task.WhenAll(tasks);

        if (summaries.Length == 1) return summaries[0];

        return await CallAiAsync(string.Join("\n\n", summaries), key);
    }

    private async Task<string> CallAiAsync(string text, string apiKey)
    {
        var kernel = kernelFactory.Create(apiKey);
        var chat = kernel.GetRequiredService<IChatCompletionService>();
        ChatHistory history = [new ChatMessageContent(AuthorRole.System, SystemPrompt)];
        history.AddUserMessage(text);
        var result = await chat.GetChatMessageContentAsync(history);
        return result?.ToString() ?? string.Empty;
    }
}