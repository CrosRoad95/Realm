namespace Realm.Common.Utilities;

public class Latch
{
    private TaskCompletionSource<object> _taskCompletionSource = new();
    private int _count = 0;
    private Task Task => _taskCompletionSource.Task;

    public Latch(int initialCounter = 0)
    {
        _count = initialCounter;
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

    public TaskAwaiter GetAwaiter()
    {
        if (_count == 0)
            _taskCompletionSource.TrySetResult(new object());
        return Task.GetAwaiter();
    }
}
