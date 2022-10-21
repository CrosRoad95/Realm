namespace Realm.Server.Interfaces;

public interface IRPGServer
{
    event Action<Player>? PlayerJoined;

    void AssociateElement(Element element);
    TService GetRequiredService<TService>() where TService : notnull;
}
