namespace BirthdayCollator.Server.Infrastructure.Throttling;

public sealed class EndpointThrottle(int concurrency, int delayMs)
{
    private readonly SemaphoreSlim _semaphore = new(concurrency);

    private async Task EnterAsync()
    {
        await _semaphore.WaitAsync();

        if (delayMs > 0)
            await Task.Delay(delayMs);
    }

    private void Exit()
    {
        _semaphore.Release();
    }

    public async Task WaitAsync() => await EnterAsync();

    public void Release() => Exit();

    public async Task RunAsync(Func<Task> action)
    {
        await EnterAsync();
        try
        {
            await action();
        }
        finally
        {
            Exit();
        }
    }

    public async Task<T> RunAsync<T>(Func<Task<T>> action)
    {
        await EnterAsync();
        try
        {
            return await action();
        }
        finally
        {
            Exit();
        }
    }
}
