namespace Realm.Persistance.Interfaces;

public interface IRepositoryBase : IDisposable
{
    Task Commit();
}
