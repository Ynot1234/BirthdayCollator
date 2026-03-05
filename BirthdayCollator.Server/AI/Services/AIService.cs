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

    private async Task<string> SummarizeChunkAsync(string text, string key)
    {
        var kernel = kernelFactory.Create(key);

        var systemPrompt = """
        You are a professional summarization assistant.
        Your only job is to summarize the provided text.
        Do not greet the user.
        Do not introduce yourself.
        Do not add emojis.
        Only return the summary.
        """;

        var chat = kernel.GetRequiredService<IChatCompletionService>();

        ChatHistory messages = [];
        messages.AddSystemMessage(systemPrompt);
        messages.AddUserMessage(text);

        var result = await chat.GetChatMessageContentAsync(messages);
        return result.ToString();
    }


    public async Task<string> SummarizeAsync(string name, string desc, string? apiKey = null)
    {
        var key = apiKey ?? config["OpenAI:ApiKey"];

        if (string.IsNullOrWhiteSpace(key))
            return "No OpenAI API key provided.";

        const int MaxChunkSize = 4000;

        List<string> chunks = [..TextChunker.Chunk(name + desc, MaxChunkSize)];

        if (chunks.Count == 1)
            return await SummarizeChunkAsync(chunks[0], key);

        List<string> partials = [];

        foreach (string chunk in chunks)
            partials.Add(await SummarizeChunkAsync(chunk, key));

        string combined = string.Join("\n\n", partials);
        return await SummarizeChunkAsync(combined, key);
    }
}