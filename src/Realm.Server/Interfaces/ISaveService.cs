namespace Realm.Server.Interfaces;

public interface ISaveService
{
    Task<bool> Save(Entity entity);
    Task Commit();
}
