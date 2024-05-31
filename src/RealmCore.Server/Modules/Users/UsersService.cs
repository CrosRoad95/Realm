using static RealmCore.Server.Modules.Users.UsersResults;

namespace RealmCore.Server.Modules.Users;

public interface IUsersService
{
    event Action<RealmPlayer>? SignedIn;
    event Action<RealmPlayer>? SignedOut;

    Task<bool> AddToRole(RealmPlayer player, string role);
    ValueTask<bool> AuthorizePolicy(RealmPlayer player, string policy);
    Task<OneOf<LoggedIn, UserDisabled, PlayerAlreadyLoggedIn, UserAlreadyInUse>> LogIn(RealmPlayer player, UserData user, bool dontLoadData = false);
    Task<OneOf<LoggedOut, PlayerNotLoggedIn>> LogOut(RealmPlayer player, CancellationToken cancellationToken = default);
    Task<OneOf<LoggedIn, QuickLoginDisabled, UserDisabled, PlayerAlreadyLoggedIn, UserAlreadyInUse>> QuickLogin(RealmPlayer player, bool dontLoadData = false);
    Task<OneOf<Registered, FailedToRegister>> Register(string username, string password);
}

internal sealed class UsersService : IUsersService
{
    private readonly ILogger<UsersService> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuthorizationService? _authorizationService;
    private readonly IUsersInUse _activeUsers;
    private readonly IServiceProvider _serviceProvider;
    private readonly AuthorizationPoliciesProvider _authorizationPoliciesProvider;

    public event Action<RealmPlayer>? SignedIn;
    public event Action<RealmPlayer>? SignedOut;

    public UsersService(ILogger<UsersService> logger,
        IDateTimeProvider dateTimeProvider, IUsersInUse activeUsers, IServiceProvider serviceProvider, AuthorizationPoliciesProvider authorizationPoliciesProvider, IAuthorizationService? authorizationService = null)
    {
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _authorizationService = authorizationService;
        _activeUsers = activeUsers;
        _serviceProvider = serviceProvider;
        _authorizationPoliciesProvider = authorizationPoliciesProvider;
    }

    public async Task<OneOf<Registered, FailedToRegister>> Register(string username, string password)
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
            return new Registered(user.Id);
        }

        _logger.LogError("Failed to create a user {userName} because: {identityResultErrors}", username, identityResult.Errors.Select(x => x.Description));
        return new FailedToRegister();
    }

    public async Task<OneOf<LoggedIn, QuickLoginDisabled, UserDisabled, PlayerAlreadyLoggedIn, UserAlreadyInUse>> QuickLogin(RealmPlayer player, bool dontLoadData = false)
    {
        var serial = player.Client.GetSerial();
        var userManager = player.GetRequiredService<UserManager<UserData>>();
        var userData = await userManager.GetUserBySerial(serial, CancellationToken.None) ?? throw new Exception("No account found.");
        
        if (!userData.QuickLogin)
            return new QuickLoginDisabled();

        var result = await LogIn(player, userData, dontLoadData);

        return result.Match<OneOf<LoggedIn, QuickLoginDisabled, UserDisabled, PlayerAlreadyLoggedIn, UserAlreadyInUse>>(loggedIn => loggedIn,
            userDisabled => userDisabled,
            playerAlreadyLoggedIn => playerAlreadyLoggedIn,
            userAlreadyInUse => userAlreadyInUse
            );
    }

    public async Task<bool> AddToRole(RealmPlayer player, string role)
    {
        var userManager = player.GetRequiredService<UserManager<UserData>>();
        var result = await userManager.AddToRoleAsync(player.User.UserData, role);
        if (result.Succeeded)
        {
            player.User.AddRole(role);
            await AuthorizePolicies(player);
            return true;
        }
        return false;
    }

    public async Task<OneOf<LoggedIn, UserDisabled, PlayerAlreadyLoggedIn, UserAlreadyInUse>> LogIn(RealmPlayer player, UserData user, bool dontLoadData = false)
    {
        using var _ = _logger.BeginElement(player);

        if (user.IsDisabled)
            return new UserDisabled(user.Id);

        if (player.User.IsLoggedIn)
            return new PlayerAlreadyLoggedIn();

        if (!_activeUsers.TrySetActive(user.Id, player))
            return new UserAlreadyInUse();

        var userManager = player.GetRequiredService<UserManager<UserData>>();
        var signInManager = player.GetRequiredService<SignInManager<UserData>>();
        var userLoginHistoryRepository = player.GetRequiredService<IUserLoginHistoryRepository>();

        // TODO: Fix it
        //user.Settings = await player.GetRequiredService<IDb>().UserSettings.Where(x => x.UserId == user.Id).ToListAsync(cancellationToken);

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
            player.User.Login(user, claimsPrincipal, dontLoadData);

            await AuthorizePolicies(player);
            await userLoginHistoryRepository.Add(user.Id, _dateTimeProvider.Now, player.Client.IPAddress?.ToString() ?? "", serial);
            UpdateLastData(player);

            await player.GetRequiredService<IPlayerUserService>().TryUpdateLastNickname(user.Id, player.Name);
            SignedIn?.Invoke(player);
            return new LoggedIn(user.Id);
        }
        catch (Exception ex)
        {
            if (player.User.IsLoggedIn)
                player.User.LogOut();
            _activeUsers.TrySetInactive(user.Id);
            _logger.LogError(ex, "Failed to sign in a user.");
            throw;
        }
    }

    public async Task<OneOf<LoggedOut, PlayerNotLoggedIn>> LogOut(RealmPlayer player, CancellationToken cancellationToken = default)
    {
        if (!player.User.IsLoggedIn)
            return new PlayerNotLoggedIn();

        var id = player.UserId;
        _activeUsers.TrySetInactive(id);

        await player.GetRequiredService<IElementSaveService>().Save(cancellationToken);
        player.User.LogOut();
        player.RemoveFromVehicle();
        player.Position = new Vector3(6000, 6000, 99999);
        player.Interior = 0;
        player.Dimension = 0;
        SignedOut?.Invoke(player);
        return new LoggedOut(id);
    }

    public async ValueTask<bool> AuthorizePolicy(RealmPlayer player, string policy)
    {
        if (_authorizationService == null)
            throw new NotSupportedException();

        var result = await _authorizationService.AuthorizeAsync(player.User.ClaimsPrincipal, policy);
        player.User.SetAuthorizedPolicyState(policy, result.Succeeded);
        return result.Succeeded;
    }

    private async Task<string?> AuthorizePolicies(RealmPlayer player)
    {
        foreach (var policy in _authorizationPoliciesProvider.Policies)
            if (!await AuthorizePolicy(player, policy))
                return policy;
        return null;
    }

    private void UpdateLastData(RealmPlayer player)
    {
        var user = player.User.UserData;
        if (user != null)
        {
            user.LastLoginDateTime = _dateTimeProvider.Now;
            var client = player.Client;
            user.LastIp = client.IPAddress?.ToString();
            user.LastSerial = client.Serial;
            user.RegisterSerial ??= client.Serial;
            user.RegisterIp ??= user.LastIp;
            user.RegisteredDateTime ??= _dateTimeProvider.Now;
            user.Nick = player.Name;
        }
    }
}
