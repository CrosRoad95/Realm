using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Realm.Persistance;

public interface IDb : IDisposable
{
    ChangeTracker ChangeTracker { get; }
    EntityEntry Attach(object entity);
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<IdentityUserClaim<int>> UserClaims { get; }
    DbSet<IdentityUserLogin<int>> UserLogins { get; }
    DbSet<IdentityRoleClaim<int>> RoleClaims { get; }
    DbSet<IdentityUserToken<int>> UserTokens { get; }
    DbSet<UserLicense> UserLicenses { get; }
    DbSet<Vehicle> Vehicles { get; }
    DbSet<VehiclePlayerAccess> VehiclePlayerAccesses { get; }
    DbSet<Inventory> Inventories { get; }
    DbSet<InventoryItem> InventoryItems { get; }
    DbSet<VehicleUpgrade> VehicleUpgrades { get; }
    DbSet<VehicleFuel> VehicleFuels { get; }
    DbSet<DailyVisits> DailyVisits { get; }
    DbSet<UserStat> UserStats { get; }
    DbSet<JobUpgrade> JobUpgrades { get; }
    DbSet<Achievement> Achievements { get; }
    DbSet<Group> Groups { get; }
    DbSet<GroupMember> GroupMembers { get; }
    DbSet<Fraction> Fractions { get; }
    DbSet<FractionMember> FractionMembers { get; }
    DbSet<UserUpgrade> UserUpgrades { get; }
    DbSet<Ban> Bans { get; }
    DbSet<JobStatistics> JobPoints { get; }
    DbSet<UserReward> UserRewards { get; }
    DbSet<UserSetting> UserSettings { get; }
    DbSet<UserWhitelistedSerial> UserWhitelistedSerials { get; }
    DbSet<UserInventory> UserInventories { get; }

    Task MigrateAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

    event EventHandler<SavedChangesEventArgs>? SavedChanges;
}
