using BirthdayCollator.Server.AI.Utilities;
using BirthdayCollator.Server.Processing.Builders;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BirthdayCollator.Server.AI.Services;

public interface IAIService
{
    Task<string> SummarizeAsync(string name, string desc, string? apiKey = null);
}

public class AIService(IConfiguration config, IKernelFactory kernelFactory) : IAIService
{
    private const string SystemPrompt = """
        You are a professional biographical summarization assistant.

        Your job is to produce a clear, well‑written summary of the person based on the provided text.
        The summary should be longer, more detailed, and include examples of their work, achievements, 
        or contributions when such information is present in the input.

        Do not greet the user.
        Do not introduce yourself.
        Do not add emojis.
        Only return the summary text.
        """;

    private string? ResolveApiKey(string? userKey)
    {
        if (!string.IsNullOrWhiteSpace(userKey))
            return userKey;

        var serverKey = config["OpenAI:ApiKey"];
        if (!string.IsNullOrWhiteSpace(serverKey))
            return serverKey;

        return null;
    }

    private async Task<string> SummarizeChunkAsync(string text, string apiKey)
    {
        var kernel = kernelFactory.Create(apiKey);

        ChatHistory messages = [];
        messages.AddSystemMessage(SystemPrompt);
        messages.AddUserMessage(text);

        var chat = kernel.GetRequiredService<IChatCompletionService>();
        var result = await chat.GetChatMessageContentAsync(messages);

        return result.ToString();
    }

    private async Task<string> SummarizeChunksAsync(List<string> chunks, string apiKey)
    {
        if (chunks.Count == 1)
            return await SummarizeChunkAsync(chunks[0], apiKey);

        List<string> partials = [];

        foreach (var chunk in chunks)
            partials.Add(await SummarizeChunkAsync(chunk, apiKey));

        string combined = string.Join("\n\n", partials);
        return await SummarizeChunkAsync(combined, apiKey);
    }

    public async Task<string> SummarizeAsync(string name, string desc, string? apiKey = null)
    {
        var key = ResolveApiKey(apiKey);
        if (string.IsNullOrWhiteSpace(key))
            return "No OpenAI API key provided.";

        const int MaxChunkSize = 4000;

        string fullText = $"{name} {desc}".Trim();
        List<string> chunks = [.. TextChunker.Chunk(fullText, MaxChunkSize)];

        return await SummarizeChunksAsync(chunks, key);
    }
}