namespace RealmCore.Server.Modules.Players.Gui.Browser;

internal sealed class BrowserGuiHostedService : PlayerLifecycle, IHostedService
{
    private readonly ILogger<BrowserGuiHostedService> _logger;
    private readonly IBrowserService _browserService;
    private readonly IBrowserGuiService _browserGuiService;
    private readonly IOptions<BrowserOptions> _browserOptions;

    public BrowserGuiHostedService(PlayersEventManager playersEventManager, ILogger<BrowserGuiHostedService> logger, IBrowserService browserService, IBrowserGuiService browserGuiService, IOptions<BrowserOptions> browserOptions) : base(playersEventManager)
    {
        _logger = logger;
        _browserService = browserService;
        _browserGuiService = browserGuiService;
        _browserOptions = browserOptions;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _browserService.BrowserLoaded += HandleBrowserLoaded;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _browserService.BrowserLoaded -= HandleBrowserLoaded;
        return Task.CompletedTask;
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
        using var _ = _logger.BeginElement(player);
        if (previousGui is BrowserGui previousBrowserGui)
        {
            var browserService = player.Browser;
            browserService.TryClose();

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
        player.Gui.Changed += HandleGuiChanged;
    }

    protected override void PlayerLeft(RealmPlayer player)
    {
        player.Gui.Changed -= HandleGuiChanged;
    }
}
