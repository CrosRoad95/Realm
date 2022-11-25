using Realm.Resources.AdminTools;

namespace Realm.Server.Logic.Resources;

public class AdminToolsLogic
{
    private readonly MtaServer _mtaServer;
    private readonly AdminToolsService _adminToolsService;

    public AdminToolsLogic(MtaServer mtaServer, AdminToolsService adminToolsService)
    {
        _mtaServer = mtaServer;
        _adminToolsService = adminToolsService;
        _mtaServer.PlayerJoined += _mtaServer_PlayerJoined;
    }

    private void _mtaServer_PlayerJoined(Player player)
    {
        ((RPGPlayer)player).AdminToolsStateChanged += AdminToolsLogic_AdminToolsStateChanged;
    }

    private void AdminToolsLogic_AdminToolsStateChanged(RPGPlayer player, bool enabled)
    {
        if (enabled)
        {
            _adminToolsService.EnableAdminToolsForPlayer(player);
        }
        else
        {
            _adminToolsService.DisableAdminToolsForPlayer(player);
        }
    }
}
