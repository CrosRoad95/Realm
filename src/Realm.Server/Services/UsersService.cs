using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Realm.Domain.Enums;
using Realm.Domain.Inventory;
using Realm.Domain.Options;

namespace Realm.Server.Services;

internal class UsersService : IUsersService
{
    private readonly ItemsRegistry _itemsRegistry;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<UsersService> _logger;
    private readonly IOptions<GameplayOptions> _gameplayOptions;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDb _db;
    private readonly IActiveUsers _activeUsers;

    public UsersService(ItemsRegistry itemsRegistry, UserManager<User> userManager, ILogger<UsersService> logger, IOptions<GameplayOptions> gameplayOptions,
        IDateTimeProvider dateTimeProvider, IAuthorizationService authorizationService, IDb db, IActiveUsers activeUsers)
    {
        _itemsRegistry = itemsRegistry;
        _userManager = userManager;
        _logger = logger;
        _gameplayOptions = gameplayOptions;
        _dateTimeProvider = dateTimeProvider;
        _authorizationService = authorizationService;
        _db = db;
        _activeUsers = activeUsers;
    }

    public async Task<int> SignUp(string username, string password)
    {
        var user = new User
        {
            UserName = username,
        };

        var identityResult = await _userManager.CreateAsync(user, password);
        if (identityResult.Succeeded)
        {
            _logger.LogInformation("Created a user account of id {userId} {userName}", user.Id, username);
            return user.Id;
        }

        _logger.LogError("Failed to create a user account {userName} because: {identityResultErrors}", username, identityResult.Errors.Select(x => x.Description));
        throw new Exception("Failed to create a user account");
    }

    public async Task<bool> SignIn(Entity entity, User user)
    {
        if (entity == null)
            throw new NullReferenceException(nameof(entity));

        if (entity.Tag != EntityTag.Player || !entity.HasComponent<PlayerElementComponent>())
            throw new NotSupportedException("Entity is not a player entity.");

        using var transaction = entity.BeginComponentTransaction();
        try
        {
            if (!_activeUsers.TrySetActive(user.Id))
                return false;

            await entity.AddComponentAsync(new AccountComponent(user));
            if (user.Inventories != null && user.Inventories.Any())
            {
                foreach (var inventory in user.Inventories)
                {
                    var items = inventory.InventoryItems
                        .Select(x =>
                            new Item(_itemsRegistry, x.ItemId, x.Number, JsonConvert.DeserializeObject<Dictionary<string, object>>(x.MetaData))
                        )
                        .ToList();
                    entity.AddComponent(new InventoryComponent(inventory.Size, inventory.Id, items));
                }
            }
            else
                entity.AddComponent(new InventoryComponent(_gameplayOptions.Value.DefaultInventorySize));

            if (user.DailyVisits != null)
                entity.AddComponent(new DailyVisitsCounterComponent(user.DailyVisits));
            else
                entity.AddComponent<DailyVisitsCounterComponent>();
            
            if (user.Stats != null)
                entity.AddComponent(new StatisticsCounterComponent(user.Stats));
            else
                entity.AddComponent<StatisticsCounterComponent>();
            
            if (user.Achievements != null)
                entity.AddComponent(new AchievementsComponent(user.Achievements));
            else
                entity.AddComponent<AchievementsComponent>();
            
            if (user.JobUpgrades != null)
                entity.AddComponent(new JobUpgradesComponent(user.JobUpgrades));
            else
                entity.AddComponent<JobUpgradesComponent>();
            
            if (user.JobStatistics != null)
                entity.AddComponent(new JobStatisticsComponent(_dateTimeProvider.Now, user.JobStatistics));
            else
                entity.AddComponent(new JobStatisticsComponent(_dateTimeProvider.Now));
            
            if (user.Discoveries != null)
                entity.AddComponent(new DiscoveriesComponent(user.Discoveries));
            else
                entity.AddComponent<DiscoveriesComponent>();
            
            if (user.DiscordIntegration != null)
                entity.AddComponent(new DiscordIntegrationComponent(user.DiscordIntegration.DiscordUserId));

            foreach (var groupMemberData in user.GroupMembers)
                entity.AddComponent(new GroupMemberComponent(groupMemberData));
            
            foreach (var fractionMemberData in user.FractionMembers)
                entity.AddComponent(new FractionMemberComponent(fractionMemberData));

            entity.AddComponent(new LicensesComponent(user.Licenses));
            entity.AddComponent(new PlayTimeComponent(user.PlayTime));
            entity.AddComponent(new LevelComponent(user.Level, user.Experience));
            entity.AddComponent(new MoneyComponent(user.Money));
            entity.AddComponent<AFKComponent>();
            entity.Commit(transaction);
        }
        catch (Exception ex)
        {
            _activeUsers.TrySetInactive(user.Id);
            entity.Rollback(transaction);
            _logger.LogError(ex, "Failed to sign in user of id {userId}", user.Id);
            throw;
        }
        entity.Disposed += HandleDestroyed;
        return true;
    }

    private void HandleDestroyed(Entity entity)
    {
        try
        {
            var accountComponent = entity.GetRequiredComponent<AccountComponent>();
            _activeUsers.TrySetInactive(accountComponent.Id);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to destroy player entity {entityName}", entity.Name);
            throw;
        }
    }

    public async Task<bool> AuthorizePolicy(AccountComponent accountComponent, string policy)
    {
        var result = await _authorizationService.AuthorizeAsync(accountComponent.ClaimsPrincipal, policy);
        return result.Succeeded;
    }

    public Task<User?> GetUserByLogin(string login)
    {
        return _userManager.Users
            .IncludeAll()
            .Where(u => u.UserName == login)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync();
    }
    
    public Task<User?> GetUserByLoginCaseInsensitive(string login)
    {
        return _userManager.Users
            .IncludeAll()
            .Where(u => u.NormalizedUserName == login.ToUpper())
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync();
    }
    
    public Task<User?> GetUserById(int id)
    {
        return _userManager.Users
            .IncludeAll()
            .Where(u => u.Id == id)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync();
    }

    public Task<bool> CheckPasswordAsync(User user, string password)
    {
        return _userManager.CheckPasswordAsync(user, password);
    }

    public Task<bool> IsUserNameInUse(string userName)
    {
        return _userManager.Users.AnyAsync(u => u.UserName == userName);
    }
    
    public Task<bool> IsUserNameInUseCaseInsensitive(string userName)
    {
        return _userManager.Users.AnyAsync(u => u.NormalizedUserName == userName.ToUpper());
    }

    public Task<bool> IsSerialWhitelisted(int userId, string serial)
    {
        return _db.UserWhitelistedSerials.AnyAsync(x => x.UserId == userId && x.Serial == serial);
    }

    public async Task<bool> TryAddWhitelistedSerial(int userId, string serial)
    {
        if (serial.Length != 32)
            throw new ArgumentException(null, nameof(serial));

        try
        {
            _db.UserWhitelistedSerials.Add(new UserWhitelistedSerial
            {
                Serial = serial,
                UserId = userId
            });

            var added = await _db.SaveChangesAsync();
            return added > 0;
        }
        catch(Exception)
        {
            return false;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    public async Task<bool> TryRemoveWhitelistedSerial(int userId, string serial)
    {
        if (serial.Length != 32)
            throw new ArgumentException(null, nameof(serial));

        try
        {
            var deleted = await _db.UserWhitelistedSerials
                .Where(x => x.UserId == userId && x.Serial == serial)
                .ExecuteDeleteAsync();

            return deleted > 0;
        }
        catch(Exception)
        {
            return false;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }
}
