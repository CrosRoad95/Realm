namespace RealmCore.Server.Logic.Components;

internal sealed class UserComponentLogic : ComponentLogic<UserComponent>
{
    private readonly IActiveUsers _activeUsers;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<UserComponentLogic> _logger;

    public UserComponentLogic(IElementFactory elementFactory, IActiveUsers activeUsers, IDateTimeProvider dateTimeProvider, ILogger<UserComponentLogic> logger) : base(elementFactory)
    {
        _activeUsers = activeUsers;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    private async Task<string?> ValidatePolicies(UserComponent userComponent)
    {
        var player = (RealmPlayer)userComponent.Element;
        var usersService = player.ServiceProvider.GetRequiredService<IUsersService>();
        var authorizationPoliciesProvider = player.ServiceProvider.GetRequiredService<AuthorizationPoliciesProvider>();
        foreach (var policy in authorizationPoliciesProvider.Policies)
            if (!await usersService.AuthorizePolicy(userComponent, policy))
                return policy;
        return null;
    }

    private async Task ComponentAddedCore(UserComponent userComponent)
    {
        await ValidatePolicies(userComponent);
        var player = (RealmPlayer)userComponent.Element;

        var userManager = player.ServiceProvider.GetRequiredService<UserManager<UserData>>();
        var user = await userManager.GetUserById(userComponent.Id);
        if(user != null)
        {
            user.LastLoginDateTime = _dateTimeProvider.Now;
            var client = player.Client;
            user.LastIp = client.IPAddress?.ToString();
            user.LastSerial = client.Serial;
            user.RegisterSerial ??= client.Serial;
            user.RegisterIp ??= user.LastIp;
            user.RegisteredDateTime ??= _dateTimeProvider.Now;
            //await _userManager.UpdateAsync(user);
        }
    }

    protected override async void ComponentAdded(UserComponent userComponent)
    {
        try
        {
            using var _ = _logger.BeginElement(userComponent.Element);
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
