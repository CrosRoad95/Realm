namespace RealmCore.Interfaces.Server;

public interface IRPGServer
{
    bool IsReady { get; }

    event Action? ServerStarted;

    void AssociateElement(object element);
    TService GetRequiredService<TService>() where TService : notnull;
    Task Start();
    Task Stop();
}
