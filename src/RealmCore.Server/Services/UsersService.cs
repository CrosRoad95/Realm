using Microsoft.AspNetCore.Authorization;
using RealmCore.Server.Json.Converters;

namespace RealmCore.Server.Services;

internal sealed class UsersService : IUsersService
{
    private readonly ItemsRegistry _itemsRegistry;
    private readonly SignInManager<UserData> _signInManager;
    private readonly ILogger<UsersService> _logger;
    private readonly IOptionsMonitor<GameplayOptions> _gameplayOptions;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuthorizationService _authorizationService;
    private readonly IActiveUsers _activeUsers;
    private readonly IElementCollection _elementCollection;
    private readonly IEntityEngine _entityEngine;
    private readonly LevelsRegistry _levelsRegistry;
    private readonly IUserRepository _userRepository;
    private readonly UserManager<UserData> _userManager;
    private readonly IUserEventRepository _userEventRepository;
    private readonly IUserLoginHistoryRepository _userLoginHistoryRepository;
    private readonly ISaveService _saveService;
    private static readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        Converters = new List<JsonConverter> { DoubleConverter.Instance }
    };

    public event Action<Entity>? SignedIn;
    public event Action<Entity>? SignedOut;

    public UsersService(ItemsRegistry itemsRegistry, SignInManager<UserData> signInManager, ILogger<UsersService> logger, IOptionsMonitor<GameplayOptions> gameplayOptions,
        IDateTimeProvider dateTimeProvider, IAuthorizationService authorizationService, IActiveUsers activeUsers, IElementCollection elementCollection, IEntityEngine ecs, LevelsRegistry levelsRegistry, IUserRepository userRepository, UserManager<UserData> userManager, IUserEventRepository userEventRepository, IUserLoginHistoryRepository userLoginHistoryRepository, ISaveService saveService)
    {
        _itemsRegistry = itemsRegistry;
        _signInManager = signInManager;
        _logger = logger;
        _gameplayOptions = gameplayOptions;
        _dateTimeProvider = dateTimeProvider;
        _authorizationService = authorizationService;
        _activeUsers = activeUsers;
        _elementCollection = elementCollection;
        _entityEngine = ecs;
        _levelsRegistry = levelsRegistry;
        _userRepository = userRepository;
        _userManager = userManager;
        _userEventRepository = userEventRepository;
        _userLoginHistoryRepository = userLoginHistoryRepository;
        _saveService = saveService;
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
        var serial = entity.GetPlayer().Client.Serial ?? throw new InvalidOperationException();
        var userData = await _userRepository.GetUserBySerial(serial) ?? throw new Exception("No account found.");
        if (!userData.QuickLogin)
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

        if (!_activeUsers.TrySetActive(user.Id, entity))
            throw new Exception("Failed to login to already active account.");

        try
        {
            await entity.AddComponentAsync(new UserComponent(user, _signInManager, _userManager));
            if (user.Inventories != null && user.Inventories.Count != 0)
            {
                foreach (var inventory in user.Inventories)
                {
                    var items = inventory.InventoryItems
                        .Select(x =>
                            new Item(_itemsRegistry, x.ItemId, x.Number, JsonConvert.DeserializeObject<Metadata>(x.MetaData, _jsonSerializerSettings))
                        )
                        .ToList();
                    entity.AddComponent(new InventoryComponent(inventory.Size, inventory.Id, items));
                }
            }
            else
                entity.AddComponent(new InventoryComponent(_gameplayOptions.CurrentValue.DefaultInventorySize));

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
            entity.AddComponent(new MoneyComponent(user.Money, _gameplayOptions));
            entity.AddComponent<AFKComponent>();
            
            var client = entity.GetPlayer().Client;
            await _userLoginHistoryRepository.Add(user.Id, _dateTimeProvider.Now, client.IPAddress?.ToString() ?? "", client.GetSerial());
            await TryUpdateLastNickName(entity);
            SignedIn?.Invoke(entity);
            return true;

        }
        catch (Exception ex)
        {
            while (entity.TryDestroyComponent<InventoryComponent>()) { }
            entity.TryDestroyComponent<DailyVisitsCounterComponent>();
            entity.TryDestroyComponent<StatisticsCounterComponent>();
            entity.TryDestroyComponent<AchievementsComponent>();
            entity.TryDestroyComponent<JobUpgradesComponent>();
            entity.TryDestroyComponent<JobStatisticsComponent>();
            entity.TryDestroyComponent<DiscoveriesComponent>();
            entity.TryDestroyComponent<DiscordIntegrationComponent>();
            while (entity.TryDestroyComponent<GroupMemberComponent>()) { }
            while (entity.TryDestroyComponent<FractionMemberComponent>()) { }
            entity.TryDestroyComponent<LicensesComponent>();
            entity.TryDestroyComponent<PlayTimeComponent>();
            entity.TryDestroyComponent<LevelComponent>();
            entity.TryDestroyComponent<MoneyComponent>();
            entity.TryDestroyComponent<AFKComponent>();
            entity.TryDestroyComponent<UserComponent>();
            _logger.LogError(ex, "Failed to sign in a user.");
            return false;
        }
    }

    public async Task SignOut(Entity entity)
    {
        var playerElementComponent = entity.GetRequiredComponent<PlayerElementComponent>();
        await _saveService.Save(entity);
        while (entity.TryDestroyComponent<InventoryComponent>()) { }
        entity.TryDestroyComponent<DailyVisitsCounterComponent>();
        entity.TryDestroyComponent<StatisticsCounterComponent>();
        entity.TryDestroyComponent<AchievementsComponent>();
        entity.TryDestroyComponent<JobUpgradesComponent>();
        entity.TryDestroyComponent<JobStatisticsComponent>();
        entity.TryDestroyComponent<DiscoveriesComponent>();
        entity.TryDestroyComponent<DiscordIntegrationComponent>();
        while (entity.TryDestroyComponent<GroupMemberComponent>()) { }
        while (entity.TryDestroyComponent<FractionMemberComponent>()) { }
        entity.TryDestroyComponent<LicensesComponent>();
        entity.TryDestroyComponent<PlayTimeComponent>();
        entity.TryDestroyComponent<LevelComponent>();
        entity.TryDestroyComponent<MoneyComponent>();
        entity.TryDestroyComponent<AFKComponent>();
        entity.TryDestroyComponent<UserComponent>();
        playerElementComponent.Player.RemoveFromVehicle();
        playerElementComponent.Spawned = false;
        entity.Transform.Position = new Vector3(6000, 6000, 99999);
        SignedOut?.Invoke(entity);
    }

    public async ValueTask<bool> AuthorizePolicy(UserComponent userComponent, string policy, bool useCache = true)
    {
        if (useCache && userComponent.HasAuthorizedPolicy(policy, out bool wasAuthorized))
            return wasAuthorized;
        var result = await _authorizationService.AuthorizeAsync(userComponent.ClaimsPrincipal, policy);
        userComponent.AddAuthorizedPolicy(policy, result.Succeeded);
        return result.Succeeded;
    }

    public void Kick(Entity entity, string reason)
    {
        entity.GetPlayer().Kick(reason);
    }

    public bool TryGetPlayerByName(string name, out Entity? playerEntity)
    {
        var player = _elementCollection.GetByType<Player>().Where(x => x.Name == name).FirstOrDefault();
        if (player == null)
        {
            playerEntity = null!;
            return false;
        }
        if(_entityEngine.TryGetEntityByPlayer(player, out var foundPlayerEntity) && foundPlayerEntity != null)
        {
            playerEntity = foundPlayerEntity;
            return true;
        }
        playerEntity = null;
        return false;
    }
    
    public async Task<bool> TryUpdateLastNickName(Entity playerEntity)
    {
        var nick = playerEntity.GetPlayer().Name;
        if (playerEntity.TryGetComponent(out UserComponent userComponent) && userComponent.Nick != nick)
        {
            return await _userRepository.TryUpdateLastNickName(userComponent.Id, playerEntity.GetPlayer().Name);
        }
        return false;
    }
    
    public IEnumerable<Entity> SearchPlayersByName(string pattern)
    {
        var players = _elementCollection.GetByType<Player>().Where(x => x.Name.Contains(pattern.ToLower(), StringComparison.CurrentCultureIgnoreCase));
        foreach (var player in players)
        {
            if (_entityEngine.TryGetEntityByPlayer(player, out var playerEntity) && playerEntity != null)
                yield return playerEntity;
        }
    }

    public async Task<bool> AddUserEvent(Entity userEntity, int eventId, string? metadata = null)
    {
        if (userEntity.TryGetComponent(out UserComponent userComponent))
        {
            await _userEventRepository.AddEvent(userComponent.Id, eventId, _dateTimeProvider.Now);
            return true;
        }
        return false;
    }

    public async Task<List<UserEventData>> GetAllUserEvents(Entity userEntity, IEnumerable<int>? events = null)
    {
        if (userEntity.TryGetComponent(out UserComponent userComponent))
        {
            return await _userEventRepository.GetAllEventsByUserId(userComponent.Id, events);
        }
        return new();
    }

    public async Task<List<UserEventData>> GetLastUserEvents(Entity userEntity, int limit = 10, IEnumerable<int>? events = null)
    {
        if (userEntity.TryGetComponent(out UserComponent userComponent))
        {
            return await _userEventRepository.GetLastEventsByUserId(userComponent.Id, limit, events);
        }
        return new();
    }
}
