namespace RealmCore.Server.Interfaces;

public interface IRealmServer
{
    event Action? ServerStarted;

    public T AssociateElement<T>(T element) where T : Element;
}
