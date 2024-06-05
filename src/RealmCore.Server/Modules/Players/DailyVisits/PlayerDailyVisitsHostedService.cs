namespace RealmCore.Server.Modules.Players.DailyVisits;

internal sealed class PlayerDailyVisitsHostedService : PlayerLifecycle, IHostedService
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public PlayerDailyVisitsHostedService(PlayersEventManager playersEventManager, IDateTimeProvider dateTimeProvider) : base(playersEventManager)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected override void PlayerLoggedIn(IPlayerUserFeature userService, RealmPlayer player)
    {
        player.DailyVisits.Update(_dateTimeProvider.Now);
    }
}
