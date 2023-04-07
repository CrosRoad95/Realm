using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace RealmCore.Persistance;

public interface IDb : IDisposable
{
    ChangeTracker ChangeTracker { get; }
    EntityEntry Attach(object entity);
    DbSet<UserData> Users { get; }
    DbSet<RoleData> Roles { get; }
    DbSet<IdentityUserClaim<int>> UserClaims { get; }
    DbSet<IdentityUserLogin<int>> UserLogins { get; }
    DbSet<IdentityRoleClaim<int>> RoleClaims { get; }
    DbSet<IdentityUserToken<int>> UserTokens { get; }
    DbSet<UserLicenseData> UserLicenses { get; }
    DbSet<VehicleData> Vehicles { get; }
    DbSet<VehicleUserAccessData> VehicleUserAccess { get; }
    DbSet<InventoryData> Inventories { get; }
    DbSet<InventoryItemData> InventoryItems { get; }
    DbSet<VehicleUpgradeData> VehicleUpgrades { get; }
    DbSet<VehicleFuelData> VehicleFuels { get; }
    DbSet<DailyVisitsData> DailyVisits { get; }
    DbSet<UserStatData> UserStats { get; }
    DbSet<JobUpgradeData> JobUpgrades { get; }
    DbSet<AchievementData> Achievements { get; }
    DbSet<GroupData> Groups { get; }
    DbSet<GroupMemberData> GroupMembers { get; }
    DbSet<FractionData> Fractions { get; }
    DbSet<FractionMemberData> FractionMembers { get; }
    DbSet<UserUpgradeData> UserUpgrades { get; }
    DbSet<BanData> Bans { get; }
    DbSet<JobStatisticsData> JobPoints { get; }
    DbSet<UserRewardData> UserRewards { get; }
    DbSet<UserSettingData> UserSettings { get; }
    DbSet<UserWhitelistedSerialData> UserWhitelistedSerials { get; }
    DbSet<UserInventoryData> UserInventories { get; }
    DbSet<VehicleInventoryData> VehicleInventories { get; }

    Task MigrateAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;

    event EventHandler<SavedChangesEventArgs>? SavedChanges;
}
