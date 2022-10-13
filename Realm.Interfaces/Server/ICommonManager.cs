namespace Realm.Interfaces.Server;

public interface ICommonManager
{
    IElement[] GetAll();
    IElement? GetById(string id);
}
