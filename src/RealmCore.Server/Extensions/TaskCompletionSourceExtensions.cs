namespace RealmCore.Server.Extensions;

public static class TaskCompletionSourceExtensions
{
    public static async Task<bool> WaitWithWithout(this TaskCompletionSource taskCompletionSource, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var task = await Task.WhenAny(taskCompletionSource.Task, Task.Delay(timeout, cancellationToken));

        cancellationToken.ThrowIfCancellationRequested();

        return task == taskCompletionSource.Task;
    }
}
