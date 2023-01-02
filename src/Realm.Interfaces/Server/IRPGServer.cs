namespace Realm.Interfaces.Server;

public interface IRPGServer
{
    void AssociateElement(IElementHandle elementHandle);
    object GetRequiredService(Type type);
}
