namespace BirthdayCollator.Server.Infrastructure.Throttling;

public interface IThrottleRegistry
{
    EndpointThrottle Year { get; }
    EndpointThrottle Date { get; }
    EndpointThrottle Category { get; }
    EndpointThrottle Redirect { get; }
}

public sealed class ThrottleRegistry : IThrottleRegistry
{
    public EndpointThrottle Year { get; } = new(concurrency: 3, delayMs: 0);
    public EndpointThrottle Date { get; } = new(concurrency: 2, delayMs: 0);
    public EndpointThrottle Category { get; } = new(concurrency: 1, delayMs: 300);
    public EndpointThrottle Redirect { get; } = new(concurrency: 2, delayMs: 0);
}