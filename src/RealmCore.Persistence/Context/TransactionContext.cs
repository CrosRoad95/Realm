namespace RealmCore.Persistence.Context;

public interface ITransactionContext
{
    Task ExecuteAsync(Func<IDb, Task> operation, CancellationToken cancellationToken = default);
    Task<T> ExecuteAsync<T>(Func<IDb, Task<T>> operation, CancellationToken cancellationToken = default);
}

internal sealed class TransactionContext : ITransactionContext
{
    private readonly IsolationLevel _isolationLevel = IsolationLevel.ReadCommitted;
    private readonly IDb _db;

    public TransactionContext(IDb db)
    {
        _db = db;
    }

    public async Task ExecuteAsync(Func<IDb, Task> operation, CancellationToken cancellationToken = default)
    {
        var executionStrategy = _db.Database.CreateExecutionStrategy();

        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(_isolationLevel, cancellationToken);
            try
            {
                await operation(_db);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public async Task<T> ExecuteAsync<T>(Func<IDb, Task<T>> operation, CancellationToken cancellationToken = default)
    {
        var executionStrategy = _db.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(_isolationLevel, cancellationToken);
            try
            {
                var result = await operation(_db);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}
