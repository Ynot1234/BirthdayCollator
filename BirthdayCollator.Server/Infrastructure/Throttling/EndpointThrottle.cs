namespace BirthdayCollator.Server.Infrastructure.Throttling;

public sealed class EndpointThrottle(int concurrency, int delayMs)
{
    private readonly SemaphoreSlim _semaphore = new(concurrency);

    public async Task<T> RunAsync<T>(Func<Task<T>> action)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (delayMs > 0) await Task.Delay(delayMs);
            return await action();
        }
        finally { _semaphore.Release(); }
    }

    public async Task RunAsync(Func<Task> action) =>
        await RunAsync<bool>(async () => { await action(); return true; });
}
