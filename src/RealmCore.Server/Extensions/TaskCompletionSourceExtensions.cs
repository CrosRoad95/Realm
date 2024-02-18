namespace RealmCore.Server.Extensions;

public static class TaskCompletionSourceExtensions
{
    public static async Task<bool> WaitWithTimeout(this TaskCompletionSource taskCompletionSource, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var task = await Task.WhenAny(taskCompletionSource.Task, Task.Delay(timeout, cancellationToken));

        cancellationToken.ThrowIfCancellationRequested();

        return task == taskCompletionSource.Task;
    }
}
