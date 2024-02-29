namespace RealmCore.Server.Modules.Players;

internal sealed class PlayerBlipLogic : PlayerLifecycle
{
    public PlayerBlipLogic(MtaServer server) : base(server)
    {
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
