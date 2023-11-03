namespace RealmCore.Server.Logic.Resources;

internal sealed class AFKResourceLogic : ComponentLogic<AFKComponent>
{
    private readonly ILogger<StatisticsCounterResourceLogic> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AFKResourceLogic(IAFKService afkService, ILogger<StatisticsCounterResourceLogic> logger, IDateTimeProvider dateTimeProvider, IElementFactory elementFactory) : base(elementFactory)
    {
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        afkService.PlayerAFKStarted += HandlePlayerAFKStarted;
        afkService.PlayerAFKStopped += HandlePlayerAFKStopped;
    }

    private void HandlePlayerAFKStarted(Player plr)
    {
        var player = (RealmPlayer)plr;
        if (player.TryGetComponent(out AFKComponent afkComponent))
        {
            using var _ = _logger.BeginElement(player);
            afkComponent.HandlePlayerAFKStarted(_dateTimeProvider.Now);
        }
    }

    private void HandlePlayerAFKStopped(Player plr)
    {
        var player = (RealmPlayer)plr;
        if (player.TryGetComponent(out AFKComponent afkComponent))
        {
            using var _ = _logger.BeginElement(player);
            afkComponent.HandlePlayerAFKStopped(_dateTimeProvider.Now);
        }
    }

    protected override void ComponentAdded(AFKComponent afkComponent)
    {
        afkComponent.StateChanged += HandleStateChanged;
    }

    protected override void ComponentDetached(AFKComponent afkComponent)
    {
        afkComponent.StateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(AFKComponent afkComponent, bool isAfk, TimeSpan elapsed)
    {
        using var _ = _logger.BeginElement(afkComponent.Element);
        _logger.LogInformation("Player {isAfk} afk", isAfk ? "started" : "stopped");
    }

}
