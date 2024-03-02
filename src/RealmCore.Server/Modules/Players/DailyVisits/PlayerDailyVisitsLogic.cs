namespace RealmCore.Server.Modules.Players.DailyVisits;

internal sealed class PlayerDailyVisitsLogic : PlayerLifecycle
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public PlayerDailyVisitsLogic(MtaServer server, IDateTimeProvider dateTimeProvider) : base(server)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    protected override void PlayerSignedIn(IPlayerUserFeature userService, RealmPlayer player)
    {
        player.DailyVisits.Update(_dateTimeProvider.Now);
    }
}
