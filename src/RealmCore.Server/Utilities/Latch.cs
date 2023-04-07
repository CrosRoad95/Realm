namespace RealmCore.Server.Utilities;

public class Latch
{
    private TaskCompletionSource<object> _taskCompletionSource = new();
    private int _count = 0;
    private readonly TimeSpan? _timeout;

    private Task Task => _taskCompletionSource.Task;

    public Latch(int initialCounter = 0, TimeSpan? timeout = null)
    {
        _count = initialCounter;
        _timeout = timeout;
    }

    public void Increment()
    {
        Interlocked.Increment(ref _count);
    }

    public void Decrement()
    {
        if (Interlocked.Decrement(ref _count) == 0)
        {
            _taskCompletionSource.TrySetResult(new object());
        }
    }

    public async Task WaitAsync()
    {
        if (_count == 0)
            _taskCompletionSource.TrySetResult(new object());

        if (_timeout != null)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                var completedTask = await Task.WhenAny(Task, Task.Delay(_timeout.Value, timeoutCancellationTokenSource.Token));
                if (completedTask == Task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    await Task;
                }
                else
                {
                    throw new TimeoutException($"The operation has timed out after {_timeout:mm\\:ss}");
                }
            }
        }

        await Task;
    }
}
