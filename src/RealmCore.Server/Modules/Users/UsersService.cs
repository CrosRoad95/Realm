﻿using static RealmCore.Server.Modules.Users.UsersResults;

namespace RealmCore.Server.Modules.Users;

public interface IUsersService
{
    event Func<RealmPlayer, Task>? LoggedIn;
    event Func<RealmPlayer, Task>? LoggedOut;

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

    public event Func<RealmPlayer, Task>? LoggedIn;
    public event Func<RealmPlayer, Task>? LoggedOut;

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

       using var serviceScope = _serviceProvider.CreateScope();
        using var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<UserData>>();
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
        var userDataRepository = player.GetRequiredService<IUserDataRepository>();
        var userData = await userDataRepository.GetBySerial(serial, CancellationToken.None) ?? throw new Exception("No account found.");
        
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
        using var serviceScope = _serviceProvider.CreateScope();
        using var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<UserData>>();
        var result = await userManager.AddToRoleAsync(player.User.UserData, role);
        if (result.Succeeded)
        {
            player.User.AddRole(role);
            await AuthorizePolicies(player);
            return true;
        }
        return false;
    }

    public async Task<OneOf<LoggedIn, UserDisabled, PlayerAlreadyLoggedIn, UserAlreadyInUse>> LogIn(RealmPlayer player, UserData userData, bool dontLoadData = false)
    {
        using var _ = _logger.BeginElement(player);

        if (userData.IsDisabled)
            return new UserDisabled(userData.Id);

        if (player.User.IsLoggedIn)
            return new PlayerAlreadyLoggedIn();

        if (!_activeUsers.TrySetActive(userData.Id, player))
            return new UserAlreadyInUse();

        try
        {
            var userDataRepository = player.GetRequiredService<IUserDataRepository>();
            var roles = await userDataRepository.GetRoles(userData.Id, CancellationToken.None);
            var signInManager = player.GetRequiredService<SignInManager<UserData>>();

            var claimsPrincipal = await signInManager.CreateUserPrincipalAsync(userData);
            if (claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
                foreach (var role in roles)
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));

            await player.User.Login(userData, claimsPrincipal, dontLoadData);

            await AuthorizePolicies(player);
            UpdateLastData(player);

            await player.GetRequiredService<IPlayerUserService>().TryUpdateLastNickname(userData.Id, player.Name);
            if (LoggedIn != null)
            {
                foreach (Func<RealmPlayer, Task> item in LoggedIn.GetInvocationList())
                {
                    await item.Invoke(player);
                }
            }
            return new LoggedIn(userData.Id);
        }
        catch (Exception ex)
        {
            if (player.User.IsLoggedIn)
                await player.User.LogOut();
            _activeUsers.TrySetInactive(userData.Id);
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
        await player.User.LogOut();
        player.RemoveFromVehicle();
        player.Position = new Vector3(6000, 6000, 99999);
        player.Interior = 0;
        player.Dimension = 0;
        if(LoggedOut != null)
        {
            foreach (Func<RealmPlayer, Task> item in LoggedOut.GetInvocationList())
            {
                await item.Invoke(player);
            }
        }

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
