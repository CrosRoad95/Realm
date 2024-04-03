namespace RealmCore.Server.Modules.Players;

internal sealed class PlayersBindsLogic : PlayerLifecycle
{
    private readonly ILogger<PlayersBindsLogic> _logger;

    public PlayersBindsLogic(PlayersEventManager playersEventManager, ILogger<PlayersBindsLogic> logger) : base(playersEventManager)
    {
        _logger = logger;
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        player.BindExecuted += HandleBindExecuted;
    }

    protected override void PlayerLeft(RealmPlayer player)
    {
        player.BindExecuted -= HandleBindExecuted;
    }

    private async void HandleBindExecuted(Player plr, PlayerBindExecutedEventArgs e)
    {
        try
        {
            var player = (RealmPlayer)plr;
            await player.InternalHandleBindExecuted(e.Key, e.KeyState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle bind");
        }
    }
}
