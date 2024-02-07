using Microsoft.AspNetCore.Authorization;

namespace RealmCore.Server.Services;

internal sealed class UsersService : IUsersService
{
    private readonly ItemsRegistry _itemsRegistry;
    private readonly ILogger<UsersService> _logger;
    private readonly IOptionsMonitor<GameplayOptions> _gameplayOptions;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuthorizationService _authorizationService;
    private readonly IActiveUsers _activeUsers;
    private readonly IElementCollection _elementCollection;
    private readonly ISaveService _saveService;
    private readonly IServiceProvider _serviceProvider;

    public event Action<RealmPlayer>? SignedIn;
    public event Action<RealmPlayer>? SignedOut;

    public UsersService(ItemsRegistry itemsRegistry, ILogger<UsersService> logger, IOptionsMonitor<GameplayOptions> gameplayOptions,
        IDateTimeProvider dateTimeProvider, IAuthorizationService authorizationService, IActiveUsers activeUsers, IElementCollection elementCollection, ISaveService saveService, IServiceProvider serviceProvider)
    {
        _itemsRegistry = itemsRegistry;
        _logger = logger;
        _gameplayOptions = gameplayOptions;
        _dateTimeProvider = dateTimeProvider;
        _authorizationService = authorizationService;
        _activeUsers = activeUsers;
        _elementCollection = elementCollection;
        _saveService = saveService;
        _serviceProvider = serviceProvider;
    }

    public async Task<int> SignUp(string username, string password, CancellationToken cancellationToken = default)
    {
        var user = new UserData
        {
            UserName = username,
        };

        var userManager = _serviceProvider.GetRequiredService<UserManager<UserData>>();
        var identityResult = await userManager.CreateAsync(user, password);
        if (identityResult.Succeeded)
        {
            _logger.LogInformation("Created a user of id {userId} {userName}", user.Id, username);
            return user.Id;
        }

        _logger.LogError("Failed to create a user {userName} because: {identityResultErrors}", username, identityResult.Errors.Select(x => x.Description));
        throw new Exception("Failed to create a user");
    }

    public async Task<bool> QuickSignIn(RealmPlayer player, CancellationToken cancellationToken = default)
    {
        var serial = player.Client.GetSerial();
        var userManager = player.GetRequiredService<UserManager<UserData>>();
        var userData = await userManager.GetUserBySerial(serial, cancellationToken) ?? throw new Exception("No account found.");
        if (!userData.QuickLogin)
            throw new Exception("Quick login not enabled");

        return await SignIn(player, userData, cancellationToken);
    }

    private void UpdateLastData(RealmPlayer player)
    {
        var user = player.User.User;
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
        }
    }

    private async Task<string?> AuthorizePolicies(RealmPlayer player)
    {
        var authorizationPoliciesProvider = player.GetRequiredService<AuthorizationPoliciesProvider>();
        foreach (var policy in authorizationPoliciesProvider.Policies)
            if (!await AuthorizePolicy(player, policy))
                return policy;
        return null;
    }

    public async Task<bool> AddToRole(RealmPlayer player, string role)
    {
        var userManager = player.GetRequiredService<UserManager<UserData>>();
        var result = await userManager.AddToRoleAsync(player.User.User, role);
        if (result.Succeeded)
        {
            player.User.AddRole(role);
            return true;
        }
        return false;
    }

    public async Task<bool> SignIn(RealmPlayer player, UserData user, CancellationToken cancellationToken = default)
    {
        if (player == null)
            throw new NullReferenceException(nameof(player));

        if (user.IsDisabled)
            throw new UserDisabledException(user.Id);

        using var _ = _logger.BeginElement(player);

        if (!_activeUsers.TrySetActive(user.Id, player))
            throw new Exception("Failed to login to already active account.");

        var userManager = player.GetRequiredService<UserManager<UserData>>();
        var signInManager = player.GetRequiredService<SignInManager<UserData>>();
        var userLoginHistoryRepository = player.GetRequiredService<IUserLoginHistoryRepository>();
        var db = player.GetRequiredService<IDb>();
        try
        {
            var serial = player.Client.GetSerial();

            var roles = await userManager.GetRolesAsync(user);
            var claimsPrincipal = await signInManager.CreateUserPrincipalAsync(user);
            foreach (var role in roles)
            {
                if (claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
            player.User.SignIn(user, claimsPrincipal);


            player.Money.SetMoneyInternal(user.Money);
            await AuthorizePolicies(player);
            await userLoginHistoryRepository.Add(user.Id, _dateTimeProvider.Now, player.Client.IPAddress?.ToString() ?? "", serial, cancellationToken);
            UpdateLastData(player);
            db.Users.Update(user);
            await db.SaveChangesAsync(cancellationToken);

            SignedIn?.Invoke(player);
            return true;

        }
        catch (Exception ex)
        {
            _activeUsers.TrySetInactive(user.Id);
            if(player.User.IsSignedIn)
                player.User.SignOut();
            player.Money.SetMoneyInternal(0);
                _logger.LogError(ex, "Failed to sign in a user.");
            return false;
        }
    }

    public async Task SignOut(RealmPlayer player, CancellationToken cancellationToken = default)
    {
        if (player.User.IsSignedIn)
            player.User.SignOut();
        await _saveService.Save(player, cancellationToken);
        _activeUsers.TrySetInactive(player.UserId);
        player.RemoveFromVehicle();
        player.Position = new Vector3(6000, 6000, 99999);
        player.Interior = 0;
        player.Dimension = 0;
        player.Money.SetMoneyInternal(0);
        SignedOut?.Invoke(player);
    }

    public async ValueTask<bool> AuthorizePolicy(RealmPlayer player, string policy, bool useCache = true)
    {
        if (useCache && player.User.HasAuthorizedPolicy(policy, out bool wasAuthorized))
            return wasAuthorized;
        var result = await _authorizationService.AuthorizeAsync(player.User.ClaimsPrincipal, policy);
        player.User.AddAuthorizedPolicy(policy, result.Succeeded);
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

    public IEnumerable<RealmPlayer> SearchPlayersByName(string pattern, bool loggedIn = true)
    {
        foreach (var player in _elementCollection.GetByType<RealmPlayer>())
        {
            if (loggedIn)
            {
                if (!player.IsSignedIn)
                    continue;
            }
                
            if(player.Name.Contains(pattern.ToLower(), StringComparison.CurrentCultureIgnoreCase))
                yield return player;
        }
    }

    public bool TryFindPlayerBySerial(string serial, out RealmPlayer? foundPlayer)
    {
        if (serial.Length != 32)
            throw new ArgumentException(nameof(serial));

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
}
