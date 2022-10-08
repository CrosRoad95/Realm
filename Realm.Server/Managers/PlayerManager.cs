namespace Realm.Server.Managers;

internal class PlayerManager : IPlayerManager
{
    public event Action<IRPGPlayer>? PlayerJoined;
    public PlayerManager(IMtaServer mtaServer)
    {
        mtaServer.PlayerJoined += MtaServer_PlayerJoined;
    }

    private void MtaServer_PlayerJoined(IRPGPlayer rpgPlayer)
    {
        PlayerJoined?.Invoke(rpgPlayer);
    }
}
