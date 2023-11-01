namespace RealmCore.Server.Logic.Resources;

internal sealed class AFKResourceLogic
{
    private readonly IElementFactory _elementFactory;
    private readonly ILogger<StatisticsCounterResourceLogic> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AFKResourceLogic(IAFKService afkService, IElementFactory elementFactory, ILogger<StatisticsCounterResourceLogic> logger, IDateTimeProvider dateTimeProvider)
    {
        _elementFactory = elementFactory;
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
            _logger.LogInformation("Player started AFK");
            afkComponent.HandlePlayerAFKStarted(_dateTimeProvider.Now);
        }
    }

    private void HandlePlayerAFKStopped(Player plr)
    {
        var player = (RealmPlayer)plr;
        if (player.TryGetComponent(out AFKComponent afkComponent))
        {
            using var _ = _logger.BeginElement(player);
            _logger.LogInformation("Player stopped AFK");
            afkComponent.HandlePlayerAFKStopped(_dateTimeProvider.Now);
        }
    }
}
