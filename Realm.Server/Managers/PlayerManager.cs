namespace Realm.Server.Managers;

internal class PlayerManager : IPlayerManager
{
    public event Action<IRPGPlayer>? PlayerJoined;
    public PlayerManager(MtaServer<RPGPlayer> mtaServer)
    {
        mtaServer.PlayerJoined += MtaServer_PlayerJoined;
    }

    private void MtaServer_PlayerJoined(RPGPlayer rpgPlayer)
    {
        PlayerJoined?.Invoke(rpgPlayer);
    }
}
