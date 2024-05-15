namespace RealmCore.Server.Modules.Players;

internal sealed class PlayerBlipHostedService : PlayerLifecycle, IHostedService
{
    public PlayerBlipHostedService(PlayersEventManager playersEventManager) : base(playersEventManager)
    {
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
        player.Spawned += HandleSpawned;
    }

    private void HandleSpawned(Player plr, PlayerSpawnedEventArgs e)
    {
        var player = (RealmPlayer)plr;
        player.AddBlip(Color.White);
    }
}
