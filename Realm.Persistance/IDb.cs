namespace Realm.Persistance;

public interface IDb
{
    DbSet<Test> Tests { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
