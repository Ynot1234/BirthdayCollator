using Microsoft.SemanticKernel;

namespace BirthdayCollator.Server.Processing.Builders;

public interface IKernelFactory
{
    Kernel Create(string apiKey);
}

public class KernelFactory : IKernelFactory
{
    public Kernel Create(string apiKey)
    {
        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("gpt-4.1", apiKey);
        return builder.Build();
    }
}
