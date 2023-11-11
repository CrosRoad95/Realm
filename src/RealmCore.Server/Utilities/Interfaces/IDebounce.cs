namespace RealmCore.Server.Utilities.Interfaces;

public interface IDebounce
{
    int Milliseconds { get; set; }

    void Invoke(Action action, CancellationToken cancellationToken = default);
    Task InvokeAsync(Action action, CancellationToken cancellationToken = default);
    Task InvokeAsync(Func<Task> task, CancellationToken cancellationToken = default);
}
