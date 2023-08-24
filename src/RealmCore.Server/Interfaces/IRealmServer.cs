namespace RealmCore.Server.Interfaces;

public interface IRealmServer
{
    public T AssociateElement<T>(T element) where T : Element;
}
