using Microsoft.AspNetCore.Authorization;
using RealmCore.Persistence.Data;
using RealmCore.Persistence.Extensions;
using RealmCore.Server.Json.Converters;

namespace RealmCore.Server.Services;

internal class UsersService : IUsersService
{
    private readonly ItemsRegistry _itemsRegistry;
    private readonly UserManager<UserData> _userManager;
    private readonly ILogger<UsersService> _logger;
    private readonly IOptions<GameplayOptions> _gameplayOptions;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDb _db;
    private readonly IActiveUsers _activeUsers;
    private readonly IElementCollection _elementCollection;
    private readonly IECS _ecs;
    private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
    {
        Converters = new List<JsonConverter> { new DoubleConverter() }
    };

    public UsersService(ItemsRegistry itemsRegistry, UserManager<UserData> userManager, ILogger<UsersService> logger, IOptions<GameplayOptions> gameplayOptions,
        IDateTimeProvider dateTimeProvider, IAuthorizationService authorizationService, IDb db, IActiveUsers activeUsers, IElementCollection elementCollection, IECS ecs)
    {
        _itemsRegistry = itemsRegistry;
        _userManager = userManager;
        _logger = logger;
        _gameplayOptions = gameplayOptions;
        _dateTimeProvider = dateTimeProvider;
        _authorizationService = authorizationService;
        _db = db;
        _activeUsers = activeUsers;
        _elementCollection = elementCollection;
        _ecs = ecs;
    }

    public async Task<int> SignUp(string username, string password)
    {
        var user = new UserData
        {
            UserName = username,
        };

        var identityResult = await _userManager.CreateAsync(user, password);
        if (identityResult.Succeeded)
        {
            _logger.LogInformation("Created a user of id {userId} {userName}", user.Id, username);
            return user.Id;
        }

        _logger.LogError("Failed to create a user {userName} because: {identityResultErrors}", username, identityResult.Errors.Select(x => x.Description));
        throw new Exception("Failed to create a user");
    }

    public async Task<bool> SignIn(Entity entity, UserData user)
    {
        if (entity == null)
            throw new NullReferenceException(nameof(entity));

        if (entity.Tag != EntityTag.Player || !entity.HasComponent<PlayerElementComponent>())
            throw new NotSupportedException("Entity is not a player entity.");

        if (user.IsDisabled)
            throw new UserDisabledException(user.Id);

        using var _ = _logger.BeginEntity(entity);

        if (!_activeUsers.TrySetActive(user.Id))
            throw new Exception("Failed to login to already active account.");

        entity.PreDisposed += HandlePreDisposed;

        try
        {
            using var transaction = entity.BeginComponentTransaction();
            try
            {
                await entity.AddComponentAsync(new UserComponent(user));
                await TryUpdateLastNickName(entity);
                if (user.Inventories != null && user.Inventories.Any())
                {
                    foreach (var inventory in user.Inventories)
                    {
                        var items = inventory.InventoryItems
                            .Select(x =>
                                new Item(_itemsRegistry, x.ItemId, x.Number, JsonConvert.DeserializeObject<Dictionary<string, object>>(x.MetaData, _jsonSerializerSettings))
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
            catch (Exception)
            {
                entity.Rollback(transaction);
                throw;
            }
            return true;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sign in a user.");
            return false;
        }
    }

    private void HandlePreDisposed(Entity entity)
    {
        try
        {
            var userComponent = entity.GetRequiredComponent<UserComponent>();
            _activeUsers.TrySetInactive(userComponent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to destroy player entity {entityName}", entity.Name);
            throw;
        }
    }

    public async Task<bool> AuthorizePolicy(UserComponent userComponent, string policy)
    {
        var result = await _authorizationService.AuthorizeAsync(userComponent.ClaimsPrincipal, policy);
        return result.Succeeded;
    }

    public Task<UserData?> GetUserByLogin(string login)
    {
        return _userManager.Users
            .TagWith(nameof(UsersService))
            .IncludeAll()
            .Where(u => u.UserName == login)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync();
    }

    public Task<UserData?> GetUserByLoginCaseInsensitive(string login)
    {
        return _userManager.Users
            .TagWith(nameof(UsersService))
            .IncludeAll()
            .Where(u => u.NormalizedUserName == login.ToUpper())
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync();
    }
    
    public Task<int> CountUsersBySerial(string serial)
    {
        return _userManager.Users
            .TagWith(nameof(UsersService))
            .Where(u => u.RegisterSerial == serial)
            .AsNoTrackingWithIdentityResolution()
            .CountAsync();
    }
    
    public Task<List<UserData>> GetUsersBySerial(string serial)
    {
        return _userManager.Users
            .TagWith(nameof(UsersService))
            .IncludeAll()
            .Where(u => u.RegisterSerial == serial)
            .AsNoTrackingWithIdentityResolution()
            .ToListAsync();
    }

    public Task<UserData?> GetUserById(int id)
    {
        return _userManager.Users
            .TagWith(nameof(UsersService))
            .IncludeAll()
            .Where(u => u.Id == id)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync();
    }

    public Task<bool> CheckPasswordAsync(UserData user, string password)
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
        return _db.UserWhitelistedSerials
            .TagWith(nameof(UsersService))
            .AnyAsync(x => x.UserId == userId && x.Serial == serial);
    }

    public async Task<bool> TryAddWhitelistedSerial(int userId, string serial)
    {
        if (serial.Length != 32)
            throw new ArgumentException(null, nameof(serial));

        try
        {
            _db.UserWhitelistedSerials.Add(new UserWhitelistedSerialData
            {
                Serial = serial,
                UserId = userId
            });

            var added = await _db.SaveChangesAsync();
            return added > 0;
        }
        catch (Exception)
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
                .TagWith(nameof(UsersService))
                .Where(x => x.UserId == userId && x.Serial == serial)
                .ExecuteDeleteAsync();

            return deleted > 0;
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
    }

    public void Kick(Entity entity, string reason)
    {
        entity.Player.Kick(reason);
    }

    public bool TryGetPlayerByName(string name, out Entity playerEntity)
    {
        var player = _elementCollection.GetByType<Player>().Where(x => x.Name == name).FirstOrDefault();
        if (player == null)
        {
            playerEntity = null!;
            return false;
        }
        return _ecs.TryGetEntityByPlayer(player, out playerEntity);
    }
    
    public async Task<bool> TryUpdateLastNickName(Entity playerEntity)
    {
        var nick = playerEntity.Player.Name;
        if (playerEntity.TryGetComponent(out UserComponent userComponent) && userComponent.Nick != nick)
        {
            await _db.Users.Where(x => x.Id == userComponent.Id)
                .ExecuteUpdateAsync(x => x.SetProperty(y => y.Nick, playerEntity.Player.Name));
            return true;
        }
        return false;
    }
    
    public IEnumerable<Entity> SearchPlayersByName(string pattern)
    {
        var players = _elementCollection.GetByType<Player>().Where(x => x.Name.ToLower().Contains(pattern.ToLower()));
        foreach (var player in players)
        {
            if (_ecs.TryGetEntityByPlayer(player, out var playerEntity))
                yield return playerEntity;
        }
    }
}
