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

    protected override Task PlayerLoggedIn(PlayerUserFeature userService, RealmPlayer player)
    {
        player.DailyVisits.Update(_dateTimeProvider.Now);
        return Task.CompletedTask;
    }
}
