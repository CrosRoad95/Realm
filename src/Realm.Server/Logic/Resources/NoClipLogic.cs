namespace Realm.Server.Logic.Resources;

internal class NoClipLogic
{
    private readonly MtaServer _mtaServer;
    private readonly NoClipService _noClipService;
    private readonly ILogger _logger;

    public NoClipLogic(MtaServer mtaServer, NoClipService noClipService, ILogger logger)
    {
        _mtaServer = mtaServer;
        _noClipService = noClipService;
        _logger = logger;
        _mtaServer.PlayerJoined += HandlePlayerJoined;
    }

    private void HandlePlayerJoined(Player player)
    {
        ((RPGPlayer)player).NoClipStateChanged += HandleNoClipStateChanged;
    }

    private void HandleNoClipStateChanged(RPGPlayer rpgPlayer, bool enabled)
    {
        if (enabled)
            _logger.Verbose("Enabled no clip");
        else
            _logger.Verbose("Disabled no clip");
        _noClipService.SetEnabledTo(rpgPlayer, enabled);
    }
}
