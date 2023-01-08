namespace Realm.Interfaces.Server;

public interface IRPGServer
{
    event Action? ServerStarted;

    void AssociateElement(IElementHandle elementHandle);
    object GetRequiredService(Type type);
}
