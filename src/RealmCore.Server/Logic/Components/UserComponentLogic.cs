namespace RealmCore.Server.Logic.Components;

internal sealed class UserComponentLogic : ComponentLogic<UserComponent>
{
    private readonly IActiveUsers _activeUsers;
    private readonly UserManager<UserData> _userManager;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<UserComponentLogic> _logger;

    public UserComponentLogic(IEntityEngine entityEngine, IActiveUsers activeUsers, UserManager<UserData> userManager, IDateTimeProvider dateTimeProvider, ILogger<UserComponentLogic> logger) : base(entityEngine)
    {
        _activeUsers = activeUsers;
        _userManager = userManager;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    private async Task ComponentAddedCore(UserComponent userComponent)
    {
        var entity = userComponent.Entity;
        if (entity.TryGetComponent(out PlayerElementComponent playerElementComponent))
        {
            var user = await _userManager.GetUserById(userComponent.Id);
            if(user != null)
            {
                user.LastLoginDateTime = _dateTimeProvider.Now;
                var client = playerElementComponent.Player.Client;
                user.LastIp = client.IPAddress?.ToString();
                user.LastSerial = client.Serial;
                user.RegisterSerial ??= client.Serial;
                user.RegisterIp ??= user.LastIp;
                user.RegisteredDateTime ??= _dateTimeProvider.Now;
                await _userManager.UpdateAsync(user);
            }
        }
    }

    protected override async void ComponentAdded(UserComponent userComponent)
    {
        try
        {
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
