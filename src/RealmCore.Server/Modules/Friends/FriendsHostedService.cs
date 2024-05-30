namespace RealmCore.Server.Modules.Friends;

internal sealed class FriendsHostedService : PlayerLifecycle, IHostedService
{
    private readonly FriendsService _friendsService;
    private readonly ILogger<FriendsHostedService> _logger;

    public FriendsHostedService(PlayersEventManager playersEventManager, FriendsService friendsService, ILogger<FriendsHostedService> logger) : base(playersEventManager)
    {
        _friendsService = friendsService;
        _logger = logger;
    }

    protected override async void PlayerSignedIn(IPlayerUserFeature user, RealmPlayer player)
    {
        try
        {
            var friends = await _friendsService.GetAllFriends(user.Id);
            // TODO:
            ;
        }
        catch (Exception ex)
        {
            _logger.LogHandleError(ex);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
