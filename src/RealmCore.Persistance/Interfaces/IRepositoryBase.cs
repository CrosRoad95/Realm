namespace RealmCore.Persistance.Interfaces;

public interface IRepositoryBase : IDisposable, IAsyncDisposable
{
    Task<int> Commit();
}
