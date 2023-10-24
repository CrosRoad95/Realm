
namespace RealmCore.Persistence.Interfaces;

public interface ITransactionContext
{
    Task ExecuteAsync(Func<IDb, Task> operation, CancellationToken cancellationToken = default);
    Task<T> ExecuteAsync<T>(Func<IDb, Task<T>> operation, CancellationToken cancellationToken = default);
}
