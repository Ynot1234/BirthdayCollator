namespace BirthdayCollator.Server.Infrastructure.Throttling;

public static class Throttle
{
    public static readonly EndpointThrottle Year = new(concurrency: 3, delayMs: 0);
    public static readonly EndpointThrottle Date = new(concurrency: 2, delayMs: 0);
    public static readonly EndpointThrottle Category = new(concurrency: 1, delayMs: 300);
    public static readonly EndpointThrottle Redirect = new(concurrency: 2, delayMs: 0);
}
