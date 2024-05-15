namespace RealmCore.Server.Modules.Players;

internal sealed class PlayersBindsHostedService : PlayerLifecycle, IHostedService
{
    private readonly ILogger<PlayersBindsHostedService> _logger;

    public PlayersBindsHostedService(PlayersEventManager playersEventManager, ILogger<PlayersBindsHostedService> logger) : base(playersEventManager)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
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
