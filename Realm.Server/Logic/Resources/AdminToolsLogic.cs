using Newtonsoft.Json.Linq;

namespace Realm.Server.Logic.Resources;

internal class AdminToolsLogic
{
    private readonly MtaServer _mtaServer;
    private readonly AdminToolsService _adminToolsService;
    private readonly ILogger _logger;

    public AdminToolsLogic(MtaServer mtaServer, AdminToolsService adminToolsService, ILogger logger)
    {
        _mtaServer = mtaServer;
        _adminToolsService = adminToolsService;
        _logger = logger;
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
            _logger.Verbose("Enabled admin tools");
            _adminToolsService.EnableAdminToolsForPlayer(player);
        }
        else
        {
            _logger.Verbose("Disabled admin tools");
            _adminToolsService.DisableAdminToolsForPlayer(player);
        }
    }
}
