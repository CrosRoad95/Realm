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
        _mtaServer.PlayerJoined += HandlePlayerJoined;
    }

    private void HandlePlayerJoined(Player player)
    {
        ((RPGPlayer)player).AdminToolsStateChanged += HandleAdminToolsStateChanged;
    }

    private void HandleAdminToolsStateChanged(RPGPlayer rpgPlayer, bool enabled)
    {
        if (enabled)
        {
            _logger.Verbose("{player} enabled admin tools", rpgPlayer);
            _adminToolsService.EnableAdminToolsForPlayer(rpgPlayer);
        }
        else
        {
            _logger.Verbose("{player} disabled admin tools", rpgPlayer);
            _adminToolsService.DisableAdminToolsForPlayer(rpgPlayer);
        }
    }
}
