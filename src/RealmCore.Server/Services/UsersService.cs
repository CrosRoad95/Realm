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
    private readonly LevelsRegistry _levelsRegistry;
    private readonly UserManager<UserData> _userManager;
    private readonly IUserEventRepository _userEventRepository;
    private readonly IUserLoginHistoryRepository _userLoginHistoryRepository;
    private readonly ISaveService _saveService;
    private readonly IBanService _banService;
    private static readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        Converters = new List<JsonConverter> { DoubleConverter.Instance }
    };

    public event Action<RealmPlayer>? SignedIn;
    public event Action<RealmPlayer>? SignedOut;

    public UsersService(ItemsRegistry itemsRegistry, SignInManager<UserData> signInManager, ILogger<UsersService> logger, IOptionsMonitor<GameplayOptions> gameplayOptions,
        IDateTimeProvider dateTimeProvider, IAuthorizationService authorizationService, IActiveUsers activeUsers, IElementCollection elementCollection, LevelsRegistry levelsRegistry, UserManager<UserData> userManager, IUserEventRepository userEventRepository, IUserLoginHistoryRepository userLoginHistoryRepository, ISaveService saveService, IBanService banService)
    {
        _itemsRegistry = itemsRegistry;
        _signInManager = signInManager;
        _logger = logger;
        _gameplayOptions = gameplayOptions;
        _dateTimeProvider = dateTimeProvider;
        _authorizationService = authorizationService;
        _activeUsers = activeUsers;
        _elementCollection = elementCollection;
        _levelsRegistry = levelsRegistry;
        _userManager = userManager;
        _userEventRepository = userEventRepository;
        _userLoginHistoryRepository = userLoginHistoryRepository;
        _saveService = saveService;
        _banService = banService;
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

    public async Task<bool> QuickSignIn(RealmPlayer player)
    {
        var serial = player.Client.GetSerial();
        var userData = await _userManager.GetUserBySerial(serial) ?? throw new Exception("No account found.");
        if (!userData.QuickLogin)
            throw new Exception("Quick login not enabled");

        return await SignIn(player, userData);
    }

    private async Task UpdateLastData(RealmPlayer player)
    {
        var userManager = player.GetRequiredService<UserManager<UserData>>();
        var user = await userManager.GetUserById(player.GetUserId());
        if (user != null)
        {
            user.LastLoginDateTime = _dateTimeProvider.Now;
            var client = player.Client;
            user.LastIp = client.IPAddress?.ToString();
            user.LastSerial = client.Serial;
            user.RegisterSerial ??= client.Serial;
            user.RegisterIp ??= user.LastIp;
            user.RegisteredDateTime ??= _dateTimeProvider.Now;;
            user.Nick = player.Name;
            await _userManager.UpdateAsync(user);
        }
    }

    public async Task<bool> SignIn(RealmPlayer player, UserData user)
    {
        if (player == null)
            throw new NullReferenceException(nameof(player));

        if (user.IsDisabled)
            throw new UserDisabledException(user.Id);

        using var _ = _logger.BeginElement(player);

        if (!_activeUsers.TrySetActive(user.Id, player))
            throw new Exception("Failed to login to already active account.");

        var components = player;
        try
        {
            var serial = player.Client.GetSerial();

            var roles = await _userManager.GetRolesAsync(user);
            var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(user);
            foreach (var role in roles)
            {
                if (claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
            var bans = await _banService.GetBansByUserIdAndSerial(user.Id, serial);
            components.AddComponent(new UserComponent(user, claimsPrincipal, bans));
            if (user.Inventories != null && user.Inventories.Count != 0)
            {
                foreach (var inventory in user.Inventories)
                {
                    var items = inventory.InventoryItems
                        .Select(x =>
                            new Item(_itemsRegistry, x.ItemId, x.Number, JsonConvert.DeserializeObject<Metadata>(x.MetaData, _jsonSerializerSettings))
                        )
                        .ToList();
                    components.AddComponent(new InventoryComponent(inventory.Size, inventory.Id, items));
                }
            }
            else
                components.AddComponent(new InventoryComponent(_gameplayOptions.CurrentValue.DefaultInventorySize));

            if (user.DailyVisits != null)
                components.AddComponent(new DailyVisitsCounterComponent(user.DailyVisits));
            else
                components.AddComponent<DailyVisitsCounterComponent>();

            if (user.Stats != null)
                components.AddComponent(new StatisticsCounterComponent(user.Stats));
            else
                components.AddComponent<StatisticsCounterComponent>();

            if (user.Achievements != null)
                components.AddComponent(new AchievementsComponent(user.Achievements));
            else
                components.AddComponent<AchievementsComponent>();

            if (user.JobUpgrades != null)
                components.AddComponent(new JobUpgradesComponent(user.JobUpgrades));
            else
                components.AddComponent<JobUpgradesComponent>();

            if (user.JobStatistics != null)
                components.AddComponent(new JobStatisticsComponent(_dateTimeProvider.Now, user.JobStatistics));
            else
                components.AddComponent(new JobStatisticsComponent(_dateTimeProvider.Now));

            if (user.Discoveries != null)
                components.AddComponent(new DiscoveriesComponent(user.Discoveries));
            else
                components.AddComponent<DiscoveriesComponent>();

            if (user.DiscordIntegration != null)
                components.AddComponent(new DiscordIntegrationComponent(user.DiscordIntegration.DiscordUserId));

            foreach (var groupMemberData in user.GroupMembers)
                components.AddComponent(new GroupMemberComponent(groupMemberData));

            foreach (var fractionMemberData in user.FractionMembers)
                components.AddComponent(new FractionMemberComponent(fractionMemberData));

            components.AddComponent(new LicensesComponent(user.Licenses, _dateTimeProvider));
            components.AddComponent(new PlayTimeComponent(_dateTimeProvider, user.PlayTime));
            components.AddComponent(new LevelComponent(user.Level, user.Experience, _levelsRegistry));
            components.AddComponent(new MoneyComponent(user.Money, _gameplayOptions));
            
            await _userLoginHistoryRepository.Add(user.Id, _dateTimeProvider.Now, player.Client.IPAddress?.ToString() ?? "", serial);
            await UpdateLastData(player);
            SignedIn?.Invoke(player);
            return true;

        }
        catch (Exception ex)
        {
            while (components.TryDestroyComponent<InventoryComponent>()) { }
            components.TryDestroyComponent<DailyVisitsCounterComponent>();
            components.TryDestroyComponent<StatisticsCounterComponent>();
            components.TryDestroyComponent<AchievementsComponent>();
            components.TryDestroyComponent<JobUpgradesComponent>();
            components.TryDestroyComponent<JobStatisticsComponent>();
            components.TryDestroyComponent<DiscoveriesComponent>();
            components.TryDestroyComponent<DiscordIntegrationComponent>();
            while (components.TryDestroyComponent<GroupMemberComponent>()) { }
            while (components.TryDestroyComponent<FractionMemberComponent>()) { }
            components.TryDestroyComponent<LicensesComponent>();
            components.TryDestroyComponent<PlayTimeComponent>();
            components.TryDestroyComponent<LevelComponent>();
            components.TryDestroyComponent<MoneyComponent>();
            components.TryDestroyComponent<UserComponent>();
            _logger.LogError(ex, "Failed to sign in a user.");
            return false;
        }
    }

    public async Task SignOut(RealmPlayer player)
    {
        var components = player;
        await _saveService.Save(player);
        while (components.TryDestroyComponent<InventoryComponent>()) { }
        components.TryDestroyComponent<DailyVisitsCounterComponent>();
        components.TryDestroyComponent<StatisticsCounterComponent>();
        components.TryDestroyComponent<AchievementsComponent>();
        components.TryDestroyComponent<JobUpgradesComponent>();
        components.TryDestroyComponent<JobStatisticsComponent>();
        components.TryDestroyComponent<DiscoveriesComponent>();
        components.TryDestroyComponent<DiscordIntegrationComponent>();
        while (components.TryDestroyComponent<GroupMemberComponent>()) { }
        while (components.TryDestroyComponent<FractionMemberComponent>()) { }
        components.TryDestroyComponent<LicensesComponent>();
        components.TryDestroyComponent<PlayTimeComponent>();
        components.TryDestroyComponent<LevelComponent>();
        components.TryDestroyComponent<MoneyComponent>();
        components.TryDestroyComponent<UserComponent>();
        player.RemoveFromVehicle();
        player.Position = new Vector3(6000, 6000, 99999);
        player.Interior = 0;
        player.Dimension = 0;
        SignedOut?.Invoke(player);
    }

    public async ValueTask<bool> AuthorizePolicy(UserComponent userComponent, string policy, bool useCache = true)
    {
        if (useCache && userComponent.HasAuthorizedPolicy(policy, out bool wasAuthorized))
            return wasAuthorized;
        var result = await _authorizationService.AuthorizeAsync(userComponent.ClaimsPrincipal, policy);
        userComponent.AddAuthorizedPolicy(policy, result.Succeeded);
        return result.Succeeded;
    }

    public bool TryGetPlayerByName(string name, out RealmPlayer? foundPlayer)
    {
        var player = _elementCollection.GetByType<RealmPlayer>().Where(x => x.Name == name).FirstOrDefault();
        if (player == null)
        {
            foundPlayer = null!;
            return false;
        }
        foundPlayer = player;
        return true;
    }

    public async Task<bool> UpdateLastNewsRead(RealmPlayer player)
    {
        if (player.TryGetComponent(out UserComponent userComponent))
        {
            var now = _dateTimeProvider.Now;
            userComponent.LastNewsReadDateTime = now;
            return await _userManager.UpdateLastNewsReadDateTime(userComponent.Id, now);
        }
        return false;
    }

    public IEnumerable<RealmPlayer> SearchPlayersByName(string pattern, bool loggedIn = true)
    {
        foreach (var player in _elementCollection.GetByType<RealmPlayer>())
        {
            if (loggedIn)
            {
                if (!player.IsLoggedIn)
                    continue;
            }
                
            if(player.Name.Contains(pattern.ToLower(), StringComparison.CurrentCultureIgnoreCase))
                yield return player;
        }
    }

    public bool TryFindPlayerBySerial(string serial, out RealmPlayer? foundPlayer)
    {
        foreach (var player in _elementCollection.GetByType<RealmPlayer>())
        {
            if(player.Client.Serial == serial)
            {
                foundPlayer = player;
                return true;
            }
        }
        foundPlayer = null;
        return false;
    }

    public async Task<bool> AddUserEvent(RealmPlayer player, int eventId, string? metadata = null)
    {
        if (player.TryGetComponent(out UserComponent userComponent))
        {
            await _userEventRepository.AddEvent(userComponent.Id, eventId, _dateTimeProvider.Now);
            return true;
        }
        return false;
    }

    public async Task<List<UserEventData>> GetAllUserEvents(RealmPlayer player, IEnumerable<int>? events = null)
    {
        if (player.TryGetComponent(out UserComponent userComponent))
        {
            return await _userEventRepository.GetAllEventsByUserId(userComponent.Id, events);
        }
        return new();
    }

    public async Task<List<UserEventData>> GetLastUserEvents(RealmPlayer player, int limit = 10, IEnumerable<int>? events = null)
    {
        if (player.TryGetComponent(out UserComponent userComponent))
        {
            return await _userEventRepository.GetLastEventsByUserId(userComponent.Id, limit, events);
        }
        return new();
    }
}
