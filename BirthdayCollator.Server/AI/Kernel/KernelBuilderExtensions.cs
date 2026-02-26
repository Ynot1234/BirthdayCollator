using Microsoft.SemanticKernel;

namespace BirthdayCollator.AI.Semantic;

public static class KernelBuilderExtensions
{
    public static IServiceCollection AddSemanticKernel(this IServiceCollection services, 
                                                       IConfiguration config)
    {
        services.AddSingleton(sp =>
        {
            var builder = Kernel.CreateBuilder();

            builder.AddOpenAIChatCompletion(
                modelId: "gpt-4.1",
               apiKey: config["OpenAI:ApiKey"] 
               ?? throw new InvalidOperationException("Missing OpenAI API key")

            );

            return builder.Build();
        });

        return services;
    }
}
