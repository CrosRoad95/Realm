namespace RealmCore.Server.Policies.Interfaces;

public interface ICommandThrottlingPolicy
{
    void Execute(Action callback);
    Task ExecuteAsync(Func<CancellationToken, Task> callback, CancellationToken cancellationToken);
}
