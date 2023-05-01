namespace RealmCore.Server.Policies.Interfaces;

public interface IPolicyDrivenCommandExecutor
{
    void Execute(Action callback, string operationKey);
    Task ExecuteAsync(Func<Task> callback, string operationKey);
}
