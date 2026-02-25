using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Threading.Tasks;

namespace BirthdayCollator.AI.Services;

public interface IAIService
{
    Task<string> SummarizeAsync(string name, string desc);
}

public class AIService(Kernel kernel) : IAIService
{
    public async Task<string> SummarizeAsync(string name, string desc)
    {
        var systemPrompt = @"
                            You are a professional summarization assistant.
                            Your only job is to summarize the provided text.
                            Do not greet the user.
                            Do not introduce yourself.
                            Do not add emojis.
                            Only return the summary.";

        IChatCompletionService chat = kernel.GetRequiredService<IChatCompletionService>();

        ChatHistory messages = [];
        messages.AddSystemMessage(systemPrompt);
        messages.AddUserMessage(name + desc);
     

        ChatMessageContent result = await chat.GetChatMessageContentAsync(messages);
        return result.ToString();
    }
}
