namespace Realm.Server.Interfaces;

public interface ILoadAndSaveService
{
    Task LoadAll();
    Task<int> SaveAll();
}