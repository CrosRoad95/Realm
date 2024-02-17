using Microsoft.AspNetCore.Authorization;

namespace RealmCore.Server.Modules.Users;

public interface IUsersService
{
    event Action<RealmPlayer>? SignedIn;
    event Action<RealmPlayer>? SignedOut;

    Task<bool> AddToRole(RealmPlayer player, string role);
    ValueTask<bool> AuthorizePolicy(RealmPlayer player, string policy);
    Task<bool> QuickSignIn(RealmPlayer player, CancellationToken cancellationToken = default);
    Task<bool> SignIn(RealmPlayer player, UserData user, CancellationToken cancellationToken = default);
    Task SignOut(RealmPlayer player, CancellationToken cancellationToken = default);
    Task<int> SignUp(string username, string password, CancellationToken cancellationToken = default);
}

internal sealed class UsersService : IUsersService
{
    private readonly ILogger<UsersService> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuthorizationService? _authorizationService;
    private readonly IUsersInUse _activeUsers;
    private readonly ISaveService _saveService;
    private readonly IServiceProvider _serviceProvider;

    public event Action<RealmPlayer>? SignedIn;
    public event Action<RealmPlayer>? SignedOut;

    public UsersService(ILogger<UsersService> logger,
        IDateTimeProvider dateTimeProvider, IUsersInUse activeUsers, ISaveService saveService, IServiceProvider serviceProvider, IAuthorizationService? authorizationService = null)
    {
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _authorizationService = authorizationService;
        _activeUsers = activeUsers;
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
        var user = player.User.UserData;
        if (user != null)
        {
            user.LastLoginDateTime = _dateTimeProvider.Now;
            var client = player.Client;
            user.LastIp = client.IPAddress?.ToString();
            user.LastSerial = client.Serial;
            user.RegisterSerial ??= client.Serial;
            user.RegisterIp ??= user.LastIp;
            user.RegisteredDateTime ??= _dateTimeProvider.Now; ;
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
        var result = await userManager.AddToRoleAsync(player.User.UserData, role);
        if (result.Succeeded)
        {
            player.User.AddRole(role);
            await AuthorizePolicies(player);
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

        if (player.User.IsSignedIn)
            throw new UserAlreadySignedInException();

        using var _ = _logger.BeginElement(player);

        if (!_activeUsers.TrySetActive(user.Id, player))
            throw new Exception("Failed to login to already active account.");

        var userManager = player.GetRequiredService<UserManager<UserData>>();
        var signInManager = player.GetRequiredService<SignInManager<UserData>>();
        var userLoginHistoryRepository = player.GetRequiredService<IUserLoginHistoryRepository>();
        var saveService = player.GetRequiredService<ISaveService>();

        // TODO: Fix it
        user.Settings = await player.GetRequiredService<IDb>().UserSettings.Where(x => x.UserId == user.Id).ToListAsync(cancellationToken);

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

            await AuthorizePolicies(player);
            await userLoginHistoryRepository.Add(user.Id, _dateTimeProvider.Now, player.Client.IPAddress?.ToString() ?? "", serial, cancellationToken);
            UpdateLastData(player);
            await saveService.Save(player, cancellationToken);

            SignedIn?.Invoke(player);
            return true;

        }
        catch (Exception ex)
        {
            if (player.User.IsSignedIn)
                player.User.SignOut();
            _activeUsers.TrySetInactive(user.Id);
            _logger.LogError(ex, "Failed to sign in a user.");
            return false;
        }
    }

    public async Task SignOut(RealmPlayer player, CancellationToken cancellationToken = default)
    {
        if (!player.User.IsSignedIn)
            throw new UserNotSignedInException();

        if (!_activeUsers.TrySetInactive(player.PersistentId))
            throw new InvalidOperationException();

        await _saveService.Save(player, cancellationToken);
        player.User.SignOut();
        player.RemoveFromVehicle();
        player.Position = new Vector3(6000, 6000, 99999);
        player.Interior = 0;
        player.Dimension = 0;
        SignedOut?.Invoke(player);
    }

    public async ValueTask<bool> AuthorizePolicy(RealmPlayer player, string policy)
    {
        if (_authorizationService == null)
            throw new NotSupportedException();

        var result = await _authorizationService.AuthorizeAsync(player.User.ClaimsPrincipal, policy);
        player.User.SetAuthorizedPolicyState(policy, result.Succeeded);
        return result.Succeeded;
    }
}
