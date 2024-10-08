﻿using static RealmCore.Server.Modules.Users.UsersResults;

namespace RealmCore.Server.Modules.Users;

public sealed class UsersService
{
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private readonly IServiceScope _serviceScope;
    private readonly ILogger<UsersService> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IAuthorizationService? _authorizationService;
    private readonly UsersInUse _activeUsers;
    private readonly IServiceProvider _serviceProvider;
    private readonly AuthorizationPoliciesProvider _authorizationPoliciesProvider;
    private readonly SignInManager<UserData> _signInManager;
    private readonly UsersRepository _usersRepository;

    public event Func<RealmPlayer, Task>? LoggedIn;
    public event Func<RealmPlayer, Task>? LoggedOut;

    public UsersService(ILogger<UsersService> logger,
        IDateTimeProvider dateTimeProvider, UsersInUse activeUsers, IServiceProvider serviceProvider, AuthorizationPoliciesProvider authorizationPoliciesProvider, IAuthorizationService? authorizationService = null)
    {
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _authorizationService = authorizationService;
        _activeUsers = activeUsers;
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        _authorizationPoliciesProvider = authorizationPoliciesProvider;
        _signInManager = _serviceProvider.GetRequiredService<SignInManager<UserData>>();
        _usersRepository = _serviceProvider.GetRequiredService<UsersRepository>();
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
        var userDataRepository = player.GetRequiredService<UsersRepository>();
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
        await _semaphoreSlim.WaitAsync();
        try
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
                var userDataRepository = player.GetRequiredService<UsersRepository>();
                var roles = await userDataRepository.GetRoles(userData.Id, CancellationToken.None);

                var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(userData);

                if (claimsPrincipal.Identity is ClaimsIdentity claimsIdentity)
                    foreach (var role in roles)
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));

                await player.User.Login(userData, claimsPrincipal, dontLoadData);

                await AuthorizePolicies(player);
                UpdateLastData(player);

                await TryUpdateLastNicknameCore(userData.Id, player.Name);
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
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task<OneOf<LoggedOut, PlayerNotLoggedIn>> LogOut(RealmPlayer player, CancellationToken cancellationToken = default)
    {
        if (!player.User.IsLoggedIn)
            return new PlayerNotLoggedIn();

        var id = player.UserId;
        _activeUsers.TrySetInactive(id);

        await player.Saving.Save(cancellationToken);
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

    public async Task<UserData?> GetUserByUserName(string userName, bool includeAll = true, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await _usersRepository.GetUserByUserName(userName, _dateTimeProvider.Now, includeAll, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
    
    public async Task<UserData?> GetUserById(int id, bool includeAll = true, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await _usersRepository.GetUserById(id, _dateTimeProvider.Now, includeAll, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private async Task<bool> TryUpdateLastNicknameCore(int userId, string nick, CancellationToken cancellationToken = default)
    {
        return await _usersRepository.TryUpdateLastNickname(userId, nick, cancellationToken);
    }
    
    public async Task<bool> TryUpdateLastNickname(int userId, string nick, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await TryUpdateLastNicknameCore(userId, nick, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
    
    public async Task<string?> GetAvatar(int userId, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await _usersRepository.GetAvatar(userId, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
    
    public async Task<bool> SetAvatar(int userId, string? avatar, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await _usersRepository.SetAvatar(userId, avatar, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
    
    public async Task<bool> SetSetting(int userId, int settingId, object? value, CancellationToken cancellationToken = default)
    {
        var stringValue = JsonHelpers.Serialize(value);
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await _usersRepository.SetSetting(userId, settingId, stringValue, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
    
    public async Task<string?> GetSetting(int userId, int settingId, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await _usersRepository.GetSetting(userId, settingId, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
    
    public async Task<UserData?> GetByDiscordUserId(ulong discordUserId, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            return await _usersRepository.GetByDiscordUserId(discordUserId, cancellationToken);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
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
