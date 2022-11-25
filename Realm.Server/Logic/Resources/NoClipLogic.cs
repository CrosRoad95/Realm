namespace Realm.Server.Logic.Resources;

internal class NoClipLogic
{
    private readonly MtaServer _mtaServer;
    private readonly NoClipService _noClipService;

    public NoClipLogic(MtaServer mtaServer, NoClipService noClipService)
    {
        _mtaServer = mtaServer;
        _noClipService = noClipService;
        _mtaServer.PlayerJoined += MtaServer_PlayerJoined;
    }

    private void MtaServer_PlayerJoined(Player player)
    {
        ((RPGPlayer)player).NoClipStateChanged += NoClipLogic_NoClipStateChanged;
    }

    private void NoClipLogic_NoClipStateChanged(RPGPlayer player, bool enabled)
    {
        _noClipService.SetEnabledTo(player, enabled);
    }
}
