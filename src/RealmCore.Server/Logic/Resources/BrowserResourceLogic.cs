using RealmCore.Resources.Browser;

namespace RealmCore.Server.Logic.Resources;

internal sealed class BrowserResourceLogic : ComponentLogic<BrowserComponent>
{
    private readonly IEntityEngine _ecs;
    private readonly IBrowserService _BrowserService;
    private readonly IOptions<BrowserOptions> _browserOptions;
    private readonly IBrowserGuiService _browserGuiService;

    public BrowserResourceLogic(IEntityEngine ecs, IBrowserService BrowserService, IOptions<BrowserOptions> browserOptions, IBrowserGuiService browserGuiService) : base(ecs)
    {
        _ecs = ecs;
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
        _browserGuiService.AuthorizeEntity(key, browserComponent.Entity);

        var url = $"/realmGuiInitialize?{_browserGuiService.KeyName}={key}";
        browserComponent.SetPath(url);
    }
    private void HandlePathChanged(BrowserComponent browserComponent, string path, bool clientSide)
    {
        _BrowserService.SetPath(browserComponent.Entity.GetPlayer(), path, clientSide);
    }

    protected override void ComponentDetached(BrowserComponent browserComponent)
    {
        browserComponent.DevToolsStateChanged -= HandleDevToolsStateChanged;
        browserComponent.PathChanged -= HandlePathChanged;
        browserComponent.VisibleChanged -= HandleVisibleChanged;

        _browserGuiService.UnauthorizeEntity(browserComponent.Entity);
    }

    private void HandleVisibleChanged(BrowserComponent browserComponent, bool visible)
    {
        _BrowserService.SetVisible(browserComponent.Entity.GetPlayer(), visible);
    }

    private void HandleDevToolsStateChanged(BrowserComponent browserComponent, bool enabled)
    {
        _BrowserService.ToggleDevTools(browserComponent.Entity.GetPlayer(), enabled);
    }

    private void HandlePlayerBrowserReadyCore(Entity playerEntity)
    {
        if(!playerEntity.HasComponent<BrowserComponent>())
            playerEntity.AddComponent<BrowserComponent>();
    }

    private void HandlePlayerBrowserReady(Player player)
    {
        if (_ecs.TryGetEntityByPlayer(player, out var entity))
            HandlePlayerBrowserReadyCore(entity);
    }
}
