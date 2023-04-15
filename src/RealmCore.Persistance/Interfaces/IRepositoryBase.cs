namespace RealmCore.Persistance.Interfaces;

public interface IRepositoryBase : IDisposable
{
    Task<int> Commit();
}
