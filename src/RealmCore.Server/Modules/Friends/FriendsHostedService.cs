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

    protected override async Task PlayerLoggedIn(PlayerUserFeature user, RealmPlayer player)
    {
        var friends = await _friendsService.GetAllFriends(user.Id);
        // TODO:
        ;
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
