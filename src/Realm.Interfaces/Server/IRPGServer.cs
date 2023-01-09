namespace Realm.Interfaces.Server;

public interface IRPGServer
{
    event Action? ServerStarted;

    void AssociateElement(IElementHandle elementHandle);
    TService GetRequiredService<TService>() where TService : notnull;
    Task Start();
    Task Stop();
}
