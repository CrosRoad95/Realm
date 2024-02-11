namespace RealmCore.Server.Modules.Security;

public class Latch
{
    private TaskCompletionSource<object> _taskCompletionSource = new();
    private int _count = 0;
    private readonly TimeSpan? _timeout;

    private Task Task => _taskCompletionSource.Task;
    public int Count => _count;

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

    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        if (_count == 0)
            _taskCompletionSource.TrySetResult(new object());

        if (_timeout != null)
        {
            await Task.WhenAny(Task, Task.Delay(_timeout.Value, cancellationToken));
            cancellationToken.ThrowIfCancellationRequested();
        }

        await Task;
    }
}
