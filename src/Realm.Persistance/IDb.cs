using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Realm.Persistance;

public interface IDb : IDisposable
{
    ChangeTracker ChangeTracker { get; }
    EntityEntry Attach(object entity);
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<IdentityUserClaim<Guid>> UserClaims { get; }
    DbSet<IdentityUserLogin<Guid>> UserLogins { get; }
    DbSet<IdentityRoleClaim<Guid>> RoleClaims { get; }
    DbSet<IdentityUserToken<Guid>> UserTokens { get; }
    DbSet<UserLicense> UserLicenses { get; }
    DbSet<Vehicle> Vehicles { get; }
    DbSet<Inventory> Inventories { get; }
    DbSet<InventoryItem> InventoryItems { get; }
    DbSet<VehicleUpgrade> VehicleUpgrades { get; }
    DbSet<VehicleFuel> VehicleFuels { get; }
    DbSet<DailyVisits> DailyVisits { get; }
    DbSet<Statistics> Statistics { get; }
    DbSet<JobUpgrade> JobUpgrades { get; }
    DbSet<Achievement> Achievements { get; }
    DbSet<Group> Groups { get; }
    DbSet<GroupMember> GroupMembers { get; }

    Task MigrateAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
