namespace Realm.Persistance;

public interface IDb
{
    DbSet<Test> Tests { get; }
    DbSet<AdminGroup> AdminGroups { get; }
    DbSet<UserAccount> UserAccounts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
