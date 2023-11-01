using RealmCore.Resources.Browser;

namespace RealmCore.Server.Logic.Resources;

internal sealed class BrowserResourceLogic : ComponentLogic<BrowserComponent>
{
    private readonly IElementFactory _elementFactory;
    private readonly IBrowserService _BrowserService;
    private readonly IOptions<BrowserOptions> _browserOptions;
    private readonly IBrowserGuiService _browserGuiService;

    public BrowserResourceLogic(IElementFactory elementFactory, IBrowserService BrowserService, IOptions<BrowserOptions> browserOptions, IBrowserGuiService browserGuiService) : base(elementFactory)
    {
        _elementFactory = elementFactory;
        _BrowserService = BrowserService;
        _browserOptions = browserOptions;
        _browserGuiService = browserGuiService;
        _BrowserService.RelayPlayerBrowserReady = HandlePlayerBrowserReady;
    }

    protected override void ComponentAdded(BrowserComponent browserComponent)
    {
        browserComponent.DevToolsStateChanged += HandleDevToolsStateChanged;
        browserComponent.PathChanged += HandlePathChanged;
        browserComponent.VisibleChanged += HandleVisibleChanged;

        var key = _browserGuiService.GenerateKey();
        _browserGuiService.AuthorizePlayer(key, (RealmPlayer)browserComponent.Element);

        var url = $"/realmGuiInitialize?{_browserGuiService.KeyName}={key}";
        browserComponent.SetPath(url);
    }
    private void HandlePathChanged(BrowserComponent browserComponent, string path, bool clientSide)
    {
        if(clientSide)
            _BrowserService.SetPath((RealmPlayer)browserComponent.Element, path, clientSide);
    }

    protected override void ComponentDetached(BrowserComponent browserComponent)
    {
        browserComponent.DevToolsStateChanged -= HandleDevToolsStateChanged;
        browserComponent.PathChanged -= HandlePathChanged;
        browserComponent.VisibleChanged -= HandleVisibleChanged;

        _browserGuiService.UnauthorizePlayer((RealmPlayer)browserComponent.Element);
    }

    private void HandleVisibleChanged(BrowserComponent browserComponent, bool visible)
    {
        _BrowserService.SetVisible((RealmPlayer)browserComponent.Element, visible);
    }

    private void HandleDevToolsStateChanged(BrowserComponent browserComponent, bool enabled)
    {
        _BrowserService.ToggleDevTools((RealmPlayer)browserComponent.Element, enabled);
    }

    private void HandlePlayerBrowserReadyCore(RealmPlayer player)
    {
        if(!player.HasComponent<BrowserComponent>())
            player.AddComponent<BrowserComponent>();
    }

    private void HandlePlayerBrowserReady(Player player)
    {
        HandlePlayerBrowserReadyCore((RealmPlayer)player);
    }
}
