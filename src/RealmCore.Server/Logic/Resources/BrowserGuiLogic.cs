using RealmCore.Server.Concepts.Gui;

namespace RealmCore.Server.Logic.Resources;

internal sealed class BrowserGuiLogic : PlayerLogic
{
    private readonly IBrowserGuiService _browserGuiService;
    private readonly ILogger<BrowserGuiLogic> _logger;

    public BrowserGuiLogic(MtaServer mtaServer, IBrowserGuiService browserGuiService, ILogger<BrowserGuiLogic> logger) : base(mtaServer)
    {
        _browserGuiService = browserGuiService;
        _logger = logger;
    }

    private void HandleGuiChanged(IPlayerGuiService guiService, RealmPlayer player, IPlayerGui? previousGui, IPlayerGui? currentGui)
    {
        if(previousGui is BrowserGui previousBrowserGui)
        {
            var browserService = player.Browser;
            browserService.SetPath("/realmEmpty");
            browserService.Close();

            _logger.LogInformation("Gui {guiPageType} closed", previousBrowserGui.GetType().Name);
        }

        if(currentGui is BrowserGui currentBrowserGui)
        {
            if (_browserGuiService.TryGetKeyByPlayer(player, out var key))
            {
                var url = currentBrowserGui.Path;
                player.Browser.Open(url);
                _logger.LogInformation("Gui {guiPageType} opened", currentBrowserGui.GetType().Name);
            }
        }
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        player.Gui.Changed += HandleGuiChanged;
    }

    protected override void PlayerLeft(RealmPlayer player)
    {
        player.Gui.Changed -= HandleGuiChanged;
    }
}
