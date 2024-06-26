﻿namespace RealmCore.Server.Modules.Security;

public interface IDebounce
{
    int Milliseconds { get; set; }

    void Invoke(Action action, CancellationToken cancellationToken = default);
    Task InvokeAsync(Action action, CancellationToken cancellationToken = default);
    Task InvokeAsync(Func<Task> task, CancellationToken cancellationToken = default);
}

internal sealed class Debounce : IDebounce
{
    private int _milliseconds;
    private CancellationTokenSource? _cancelationTokenSource;
    private CancellationToken? _changedTask;

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

    public Debounce(int milliseconds)
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
        if (_cancelationTokenSource != null)
            await _cancelationTokenSource.CancelAsync();

        try
        {
            _cancelationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _changedTask = _cancelationTokenSource.Token;
            await Task.Delay(_milliseconds, _changedTask.Value);
            action();
        }
        catch (TaskCanceledException)
        {
            // Ignore
        }
    }

    public async Task InvokeAsync(Func<Task> task, CancellationToken cancellationToken = default)
    {
        if (_cancelationTokenSource != null)
            await _cancelationTokenSource.CancelAsync();

        try
        {
            _cancelationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _changedTask = _cancelationTokenSource.Token;
            await Task.Delay(_milliseconds, _changedTask.Value);
            await task();
        }
        catch (TaskCanceledException)
        {
            // Ignore
        }
    }
}
