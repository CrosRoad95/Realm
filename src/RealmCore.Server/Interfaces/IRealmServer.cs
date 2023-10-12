namespace RealmCore.Server.Interfaces;

public interface IRealmServer
{
    event Action? ServerStarted;

    T AssociateElement<T>(T element) where T : Element;
    T GetRequiredService<T>() where T : notnull;
}
