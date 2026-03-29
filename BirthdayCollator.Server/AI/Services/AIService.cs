using BirthdayCollator.Server.AI.Utilities;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.RegularExpressions;

namespace BirthdayCollator.Server.AI.Services;

public class AIService(IConfiguration config, IKernelFactory kernelFactory) : IBioService
{
    private const string SummaryPrompt =
        "You are a professional biographical summarizer. Return only the summary text. No greetings, introductions, or emojis.";

    private const string BirthYearPrompt =
      "You are a biographical researcher. Your goal is to identify a person's birth year if they were born on today's Month and Day. " +
      "1. Look at the provided context. " +
      "2. If the year isn't there, use your internal knowledge of this person. " +
      "3. Only respond with the 4-digit year (e.g., 1925). " +
      "4. If you truly cannot identify the person or their birth year, respond 'unknown'. " +
      "Ignore any other dates like death years.";

    public async Task<string> SummarizeAsync(string name, string desc, string? apiKey = null)
    {
        var key = apiKey ?? config["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(key))
            return "No OpenAI API key provided.";

        string fullText = $"{name} {desc}".Trim();
        var chunks = TextChunker.Chunk(fullText, 4000).ToList();

        var tasks = chunks.Select(c => CallAiAsync(c, key, SummaryPrompt));
        var summaries = await Task.WhenAll(tasks);

        if (summaries.Length == 1)
            return summaries[0];

        return await CallAiAsync(string.Join("\n\n", summaries), key, SummaryPrompt);
    }

    public async Task<string> ExtractBirthYearAsync(string name, string bio, string? apiKey = null)
    {
        var key = apiKey ?? config["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(key))
            return "unknown";

        string userPrompt = $"Based on this text: '{bio}', what year was {name} born?";

        var result = await CallAiAsync(userPrompt, key, BirthYearPrompt);

        var match = Regex.Match(result, @"\b(18|19|20)\d{2}\b");
        return match.Success ? match.Value : "unknown";
    }

    private async Task<string> CallAiAsync(string text, string apiKey, string systemPrompt)
    {
        var kernel = kernelFactory.Create(apiKey);
        var chat = kernel.GetRequiredService<IChatCompletionService>();

        var history = new ChatHistory
        {
            new ChatMessageContent(AuthorRole.System, systemPrompt),
            new ChatMessageContent(AuthorRole.User, text)
        };

        var result = await chat.GetChatMessageContentAsync(history);
        return result?.Content ?? string.Empty;
    }
}
