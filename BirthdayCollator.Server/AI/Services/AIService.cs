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

        string systemPrompt = """
                                You are a professional biographical summarization assistant.

                                Your job is to produce a clear, well‑written summary of the person based on the provided text.
                                The summary should be longer, more detailed, and include examples of their work, achievements, or contributions when such information is present in the input.

                                Do not greet the user.
                                Do not introduce yourself.
                                Do not add emojis.
                                Only return the summary text.
                                """;

        ChatHistory messages = [];
        messages.AddSystemMessage(systemPrompt);
        messages.AddUserMessage(text);

        var chat = kernel.GetRequiredService<IChatCompletionService>();
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