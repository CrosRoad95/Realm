using Realm.Persistance.Data;

namespace Realm.Persistance;

public interface IDb
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<IdentityUserClaim<Guid>> UserClaims { get; }
    DbSet<IdentityUserLogin<Guid>> UserLogins { get; }
    DbSet<IdentityRoleClaim<Guid>> RoleClaims { get; }
    DbSet<IdentityUserToken<Guid>> UserTokens { get; }
    DbSet<Test> Tests { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
