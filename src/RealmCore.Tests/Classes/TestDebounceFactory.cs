namespace RealmCore.Tests.Classes;

internal class TestDebounce : IDebounce
{
    private int _milliseconds;
    private SemaphoreSlim _semaphore = new(0);
    private TaskCompletionSource? _taskCompletionSource;
    public async Task Release()
    {
        _taskCompletionSource = new();
        _semaphore.Release();
        await _taskCompletionSource.Task;
    }

    public int Milliseconds
    {
        get { return _milliseconds; }
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException();
            _milliseconds = value;
        }
    }

    public TestDebounce(int milliseconds)
    {
        _milliseconds = milliseconds;
    }

    public void Invoke(Action action, CancellationToken cancellationToken = default)
    {
        Task.Run(async () =>
        {
            try
            {
                await InvokeAsync(action, cancellationToken);
            }
            catch (Exception)
            {
                // Ignore
            }
        }, CancellationToken.None);
    }

    public async Task InvokeAsync(Action action, CancellationToken cancellationToken = default)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            action();
            _taskCompletionSource?.SetResult();
        }
        catch (TaskCanceledException)
        {
            // Ignore
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task InvokeAsync(Func<Task> task, CancellationToken cancellationToken = default)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            await task();
            _taskCompletionSource?.SetResult();
        }
        catch (TaskCanceledException)
        {
            // Ignore
        }
        catch (Exception)
        {
            throw;
        }
    }
}

internal class TestDebounceFactory : IDebounceFactory
{
    public TestDebounce LastDebounce { get; private set; }
    public IDebounce Create(int milliseconds)
    {
        LastDebounce = new TestDebounce(milliseconds);
        return LastDebounce;
    }
}
