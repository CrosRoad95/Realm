using Microsoft.AspNetCore.Authorization;
using RealmCore.Server.Json.Converters;

namespace RealmCore.Server.Services;

internal sealed class UsersService : IUsersService
{
    private readonly ItemsRegistry _itemsRegistry;
    private readonly SignInManager<UserData> _signInManager;
    private readonly ILogger<UsersService> _logger;
    private readonly IOptions<GameplayOptions> _gameplayOptions;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuthorizationService _authorizationService;
    private readonly IActiveUsers _activeUsers;
    private readonly IElementCollection _elementCollection;
    private readonly IEntityEngine _ecs;
    private readonly LevelsRegistry _levelsRegistry;
    private readonly IUserRepository _userRepository;
    private readonly UserManager<UserData> _userManager;
    private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
    {
        Converters = new List<JsonConverter> { new DoubleConverter() }
    };

    public UsersService(ItemsRegistry itemsRegistry, SignInManager<UserData> signInManager, ILogger<UsersService> logger, IOptions<GameplayOptions> gameplayOptions,
        IDateTimeProvider dateTimeProvider, IAuthorizationService authorizationService, IActiveUsers activeUsers, IElementCollection elementCollection, IEntityEngine ecs, LevelsRegistry levelsRegistry, IUserRepository userRepository, UserManager<UserData> userManager)
    {
        _itemsRegistry = itemsRegistry;
        _signInManager = signInManager;
        _logger = logger;
        _gameplayOptions = gameplayOptions;
        _dateTimeProvider = dateTimeProvider;
        _authorizationService = authorizationService;
        _activeUsers = activeUsers;
        _elementCollection = elementCollection;
        _ecs = ecs;
        _levelsRegistry = levelsRegistry;
        _userRepository = userRepository;
        _userManager = userManager;
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

    public async Task<bool> QuickSignIn(Entity entity)
    {
        var serial = entity.GetPlayer().Client.Serial;
        var userData = await _userRepository.GetUserBySerial(serial);
        if(userData == null)
            throw new Exception("No account found.");

        if(!userData.QuickLogin)
            throw new Exception("Quick login not enabled");

        return await SignIn(entity, userData);
    }

    public async Task<bool> SignIn(Entity entity, UserData user)
    {
        if (entity == null)
            throw new NullReferenceException(nameof(entity));

        if (!entity.HasComponent<PlayerTagComponent>())
            throw new NotSupportedException("Entity is not a player entity.");

        if (user.IsDisabled)
            throw new UserDisabledException(user.Id);

        using var _ = _logger.BeginEntity(entity);

        if (!_activeUsers.TrySetActive(user.Id))
            throw new Exception("Failed to login to already active account.");

        entity.PreDisposed += HandlePreDisposed;

        try
        {
            await entity.AddComponentAsync(new UserComponent(user, _signInManager, _userManager));
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

            entity.AddComponent(new LicensesComponent(user.Licenses, _dateTimeProvider));
            entity.AddComponent(new PlayTimeComponent(user.PlayTime, _dateTimeProvider));
            entity.AddComponent(new LevelComponent(user.Level, user.Experience, _levelsRegistry));
            entity.AddComponent(new MoneyComponent(user.Money, _gameplayOptions.Value.MoneyLimit, _gameplayOptions.Value.MoneyPrecision));
            entity.AddComponent<AFKComponent>();
            return true;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sign in a user.");
            return false;
        }
    }

    public async Task<bool> AuthorizePolicy(UserComponent userComponent, string policy)
    {
        var result = await _authorizationService.AuthorizeAsync(userComponent.ClaimsPrincipal, policy);
        return result.Succeeded;
    }

    public void Kick(Entity entity, string reason)
    {
        entity.GetPlayer().Kick(reason);
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
        var nick = playerEntity.GetPlayer().Name;
        if (playerEntity.TryGetComponent(out UserComponent userComponent) && userComponent.Nick != nick)
        {
            return await _userRepository.TryUpdateLastNickName(userComponent.Id, playerEntity.GetPlayer().Name).ConfigureAwait(false);
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

    private void HandlePreDisposed(Entity entity)
    {
        try
        {
            var userComponent = entity.GetRequiredComponent<UserComponent>();
            _activeUsers.TrySetInactive(userComponent.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to destroy player entity");
            throw;
        }
    }
}
