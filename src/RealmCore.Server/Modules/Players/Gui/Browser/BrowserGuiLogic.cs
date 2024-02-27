namespace RealmCore.Server.Modules.Players.Gui;

internal sealed class BrowserGuiLogic : PlayerLogic
{
    private readonly ILogger<BrowserGuiLogic> _logger;
    private readonly IOptions<GuiBrowserOptions> _guiBrowserOptions;
    private readonly IBrowserService _browserService;
    private readonly IBrowserGuiService _browserGuiService;
    private readonly IOptions<BrowserOptions> _browserOptions;

    public BrowserGuiLogic(MtaServer server, ILogger<BrowserGuiLogic> logger, IOptions<GuiBrowserOptions> guiBrowserOptions, IBrowserService browserService, IBrowserGuiService browserGuiService, IOptions<BrowserOptions> browserOptions) : base(server)
    {
        _logger = logger;
        _guiBrowserOptions = guiBrowserOptions;
        _browserService = browserService;
        _browserGuiService = browserGuiService;
        _browserOptions = browserOptions;
        browserService.BrowserLoaded += HandleBrowserLoaded;
    }

    private void HandleBrowserLoaded(Player plr)
    {
        var player = (RealmPlayer)plr;

        var key = player.Browser.Key;
        if (_browserGuiService.AuthorizePlayer(key, player))
        {
            var url = $"{_browserOptions.Value.BaseRemoteUrl}/{BrowserConstants.DefaultPage}/?{BrowserConstants.QueryParameterName}={key}";
            _browserService.SetPath(player, url);
        }
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
