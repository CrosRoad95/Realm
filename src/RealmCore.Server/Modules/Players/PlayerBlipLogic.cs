namespace RealmCore.Server.Modules.Players;

internal sealed class PlayerBlipLogic : PlayerLifecycle
{
    public PlayerBlipLogic(PlayersEventManager playersEventManager) : base(playersEventManager)
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
