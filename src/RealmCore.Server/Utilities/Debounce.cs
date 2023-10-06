namespace RealmCore.Server.Utilities;

public class Debounce
{
    private readonly int _miliseconds;
    private CancellationTokenSource? _cancelationTokenSource;
    private CancellationToken? _changedTask;

    public Debounce(int miliseconds)
    {
        _miliseconds = miliseconds;
    }

    public async Task InvokeAsync(Action action, CancellationToken cancellationToken = default)
    {
        if (_cancelationTokenSource != null)
            _cancelationTokenSource.Cancel();

        try
        {
            _cancelationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _changedTask = _cancelationTokenSource.Token;
            await Task.Delay(_miliseconds, _changedTask.Value);
            action();
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
        if (_cancelationTokenSource != null)
            _cancelationTokenSource.Cancel();

        try
        {
            _cancelationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _changedTask = _cancelationTokenSource.Token;
            await Task.Delay(_miliseconds, _changedTask.Value);
            await task();
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
