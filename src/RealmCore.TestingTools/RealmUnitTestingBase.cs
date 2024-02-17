using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace RealmCore.TestingTools;

public abstract class RealmUnitTestingBase
{
    private RealmTestingServer? _server;

    protected RealmTestingServer CreateServer()
    {
        if (_server == null)
        {
            _server = new RealmTestingServer(new TestConfigurationProvider(""), services =>
            {
                services.AddScoped<IDb, TestDb>();
                services.AddScoped<IUserEventRepository>(x => null);
            });
        }
        return _server;
    }

    protected RealmPlayer CreatePlayer(string name = "TestPlayer")
    {
        if (_server == null)
            throw new Exception("Server not created.");

        var player = _server.CreatePlayer(name: name);
        if (player.IsDestroyed)
            return player;

        player.User.SignIn(new UserData
        {
            UserName = player.Name
        }, new ClaimsPrincipal
        {

        });
        return player;
    }

    protected RealmPlayer CreateServerWithOnePlayer(string name = "TestPlayer")
    {
        CreateServer();
        return CreatePlayer(name);
    }
}

public class TestDb : IDb
{
    public DatabaseFacade Database => throw new NotSupportedException();

    public ChangeTracker ChangeTracker => throw new NotSupportedException();

    public DbSet<UserData> Users => throw new NotSupportedException();

    public DbSet<RoleData> Roles => throw new NotSupportedException();

    public DbSet<IdentityUserClaim<int>> UserClaims => throw new NotSupportedException();

    public DbSet<IdentityUserLogin<int>> UserLogins => throw new NotSupportedException();

    public DbSet<IdentityRoleClaim<int>> RoleClaims => throw new NotSupportedException();

    public DbSet<IdentityUserToken<int>> UserTokens => throw new NotSupportedException();

    public DbSet<UserLicenseData> UserLicenses => throw new NotSupportedException();

    public DbSet<VehicleData> Vehicles => throw new NotSupportedException();

    public DbSet<VehicleUserAccessData> VehicleUserAccess => throw new NotSupportedException();

    public DbSet<InventoryData> Inventories => throw new NotSupportedException();

    public DbSet<InventoryItemData> InventoryItems => throw new NotSupportedException();

    public DbSet<VehicleUpgradeData> VehicleUpgrades => throw new NotSupportedException();

    public DbSet<VehicleFuelData> VehicleFuels => throw new NotSupportedException();

    public DbSet<DailyVisitsData> DailyVisits => throw new NotSupportedException();

    public DbSet<UserStatData> UserStats => throw new NotSupportedException();

    public DbSet<JobUpgradeData> JobUpgrades => throw new NotSupportedException();

    public DbSet<AchievementData> Achievements => throw new NotSupportedException();

    public DbSet<GroupData> Groups => throw new NotSupportedException();

    public DbSet<GroupMemberData> GroupMembers => throw new NotSupportedException();

    public DbSet<FractionData> Fractions => throw new NotSupportedException();

    public DbSet<FractionMemberData> FractionMembers => throw new NotSupportedException();

    public DbSet<UserUpgradeData> UserUpgrades => throw new NotSupportedException();

    public DbSet<BanData> Bans => throw new NotSupportedException();

    public DbSet<JobStatisticsData> JobPoints => throw new NotSupportedException();

    public DbSet<UserRewardData> UserRewards => throw new NotSupportedException();

    public DbSet<UserSettingData> UserSettings => throw new NotSupportedException();

    public DbSet<UserWhitelistedSerialData> UserWhitelistedSerials => throw new NotSupportedException();

    public DbSet<UserInventoryData> UserInventories => throw new NotSupportedException();

    public DbSet<VehicleInventoryData> VehicleInventories => throw new NotSupportedException();

    public DbSet<VehicleEventData> VehicleEvents => throw new NotSupportedException();

    public DbSet<UserEventData> UserEvents => throw new NotSupportedException();

    public DbSet<RatingData> Ratings => throw new NotSupportedException();

    public DbSet<OpinionData> Opinions => throw new NotSupportedException();

    public DbSet<UserNotificationData> UserNotifications => throw new NotSupportedException();

    public DbSet<UserLoginHistoryData> UserLoginHistory => throw new NotSupportedException();

    public DbSet<UserMoneyHistoryData> UserMoneyHistory => throw new NotSupportedException();

    public DbSet<NewsData> News => throw new NotSupportedException();

    public DbSet<TagData> Tags => throw new NotSupportedException();

    public DbSet<NewsTagData> NewsTags => throw new NotSupportedException();

    public event EventHandler<SavedChangesEventArgs>? SavedChanges;

    public EntityEntry Attach(object entity)
    {
        throw new NotSupportedException();
    }

    public void Dispose()
    {

    }

    public EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class
    {
        throw new NotSupportedException();
    }

    public Task MigrateAsync()
    {
        throw new NotSupportedException();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}