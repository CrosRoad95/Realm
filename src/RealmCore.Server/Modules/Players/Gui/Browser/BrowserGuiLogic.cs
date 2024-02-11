namespace RealmCore.Server.Modules.Players.Gui;

internal sealed class BrowserGuiLogic : PlayerLogic
{
    private readonly ILogger<BrowserGuiLogic> _logger;
    private readonly IOptions<GuiBrowserOptions> _guiBrowserOptions;

    public BrowserGuiLogic(MtaServer mtaServer, ILogger<BrowserGuiLogic> logger, IOptions<GuiBrowserOptions> guiBrowserOptions) : base(mtaServer)
    {
        _logger = logger;
        _guiBrowserOptions = guiBrowserOptions;
    }

    private void HandleGuiChanged(IPlayerGuiFeature guiService, RealmPlayer player, IPlayerGui? previousGui, IPlayerGui? currentGui)
    {
        if (previousGui is BrowserGui previousBrowserGui)
        {
            var browserService = player.Browser;
            browserService.Close();

            _logger.LogInformation("Browser gui {guiPageType} closed", previousBrowserGui.GetType().Name);
        }

        if (currentGui is BrowserGui currentBrowserGui)
        {
            var url = currentBrowserGui.Path;
            player.Browser.Open(url, currentBrowserGui.QueryParameters);
            _logger.LogInformation("Browser gui {guiPageType} opened at {url}", currentBrowserGui.GetType().Name, url);
        }
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        if(_guiBrowserOptions.Value.BrowserSupport)
            player.Gui.Changed += HandleGuiChanged;
    }

    protected override void PlayerLeft(RealmPlayer player)
    {
        if (_guiBrowserOptions.Value.BrowserSupport)
            player.Gui.Changed -= HandleGuiChanged;
    }
}
