namespace Realm.Server.Interfaces;

public interface ILoadAndSaveService
{
    Task LoadAll();
    ValueTask<bool> Save(Entity entity, IDb context);
}