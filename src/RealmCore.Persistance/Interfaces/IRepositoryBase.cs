namespace RealmCore.Persistence.Interfaces;

public interface IRepositoryBase : IDisposable, IAsyncDisposable
{
    Task<int> Commit();
}
