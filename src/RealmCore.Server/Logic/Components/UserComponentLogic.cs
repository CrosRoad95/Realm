namespace RealmCore.Server.Logic.Components;

internal sealed class UserComponentLogic : ComponentLogic<UserComponent>
{
    private readonly IActiveUsers _activeUsers;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<UserComponentLogic> _logger;

    public UserComponentLogic(IEntityEngine entityEngine, IActiveUsers activeUsers, IDateTimeProvider dateTimeProvider, ILogger<UserComponentLogic> logger) : base(entityEngine)
    {
        _activeUsers = activeUsers;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    private async Task<string?> ValidatePolicies(UserComponent userComponent)
    {
        var realmPlayer = (RealmPlayer)userComponent.Entity.GetPlayer();
        var usersService = realmPlayer.ServiceProvider.GetRequiredService<IUsersService>();
        var authorizationPoliciesProvider = realmPlayer.ServiceProvider.GetRequiredService<AuthorizationPoliciesProvider>();
        foreach (var policy in authorizationPoliciesProvider.Policies)
            if (!await usersService.AuthorizePolicy(userComponent, policy))
                return policy;
        return null;
    }

    private async Task ComponentAddedCore(UserComponent userComponent)
    {
        await ValidatePolicies(userComponent);
        var entity = userComponent.Entity;
        if (entity.TryGetComponent(out PlayerElementComponent playerElementComponent))
        {
            var realmPlayer = (RealmPlayer)entity.GetPlayer();
            var userManager = realmPlayer.ServiceProvider.GetRequiredService<UserManager<UserData>>();
            var user = await userManager.GetUserById(userComponent.Id);
            if(user != null)
            {
                user.LastLoginDateTime = _dateTimeProvider.Now;
                var client = playerElementComponent.Player.Client;
                user.LastIp = client.IPAddress?.ToString();
                user.LastSerial = client.Serial;
                user.RegisterSerial ??= client.Serial;
                user.RegisterIp ??= user.LastIp;
                user.RegisteredDateTime ??= _dateTimeProvider.Now;
                //await _userManager.UpdateAsync(user);
            }
        }
    }

    protected override async void ComponentAdded(UserComponent userComponent)
    {
        try
        {
            using var _ = _logger.BeginEntity(userComponent.Entity);
            await ComponentAddedCore(userComponent);
        }
        catch(Exception ex)
        {
            _logger.LogHandleError(ex);
        }
    }

    protected override void ComponentDetached(UserComponent userComponent)
    {
        _activeUsers.TrySetInactive(userComponent.Id);
    }
}
