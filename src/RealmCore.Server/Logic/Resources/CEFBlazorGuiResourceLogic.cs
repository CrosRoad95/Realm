using RealmCore.Resources.CEFBlazorGui;

namespace RealmCore.Server.Logic.Resources;

internal sealed class CEFBlazorGuiResourceLogic : ComponentLogic<BrowserComponent>
{
    private readonly IEntityEngine _ecs;
    private readonly ICEFBlazorGuiService _cefBlazorGuiService;
    private readonly IOptions<BrowserOptions> _blazorOptions;
    private readonly IBrowserGuiService _browserGuiService;

    public CEFBlazorGuiResourceLogic(IEntityEngine ecs, ICEFBlazorGuiService cefBlazorGuiService, IOptions<BrowserOptions> blazorOptions, IBrowserGuiService browserGuiService) : base(ecs)
    {
        _ecs = ecs;
        _cefBlazorGuiService = cefBlazorGuiService;
        _blazorOptions = blazorOptions;
        _browserGuiService = browserGuiService;
        _cefBlazorGuiService.RelayPlayerBrowserReady = HandlePlayerBrowserReady;
    }

    protected override void ComponentAdded(BrowserComponent blazorGuiComponent)
    {
        blazorGuiComponent.DevToolsStateChanged += HandleDevToolsStateChanged;
        blazorGuiComponent.PathChanged += HandlePathChanged;
        blazorGuiComponent.VisibleChanged += HandleVisibleChanged;
        blazorGuiComponent.RemotePathChanged += HandleRemotePathChanged;

        var key = _browserGuiService.GenerateKey();
        _browserGuiService.AuthorizeEntity(key, blazorGuiComponent.Entity);

        var url = $"/realmGuiInitialize?{_browserGuiService.KeyName}={key}";
        blazorGuiComponent.LoadRemotePage(url, false);
    }

    protected override void ComponentDetached(BrowserComponent blazorGuiComponent)
    {
        blazorGuiComponent.DevToolsStateChanged -= HandleDevToolsStateChanged;
        blazorGuiComponent.PathChanged -= HandlePathChanged;
        blazorGuiComponent.VisibleChanged -= HandleVisibleChanged;
        blazorGuiComponent.RemotePathChanged -= HandleRemotePathChanged;

        _browserGuiService.UnauthorizeEntity(blazorGuiComponent.Entity);
    }

    private void HandleRemotePathChanged(BrowserComponent blazorGuiComponent, string path)
    {
        if(_browserGuiService.TryGetKeyByEntity(blazorGuiComponent.Entity, out var key))
        {
            _cefBlazorGuiService.SetRemotePath(blazorGuiComponent.Entity.GetPlayer(), path);
        }
    }

    private void HandleVisibleChanged(BrowserComponent blazorGuiComponent, bool visible)
    {
        _cefBlazorGuiService.SetVisible(blazorGuiComponent.Entity.GetPlayer(), visible);
    }

    private void HandlePathChanged(BrowserComponent blazorGuiComponent, string? path, bool force, GuiPageType guiType, GuiPageChangeSource guiPageChangeSource)
    {
        if(guiPageChangeSource == GuiPageChangeSource.Server)
            _cefBlazorGuiService.SetPath(blazorGuiComponent.Entity.GetPlayer(), path ?? "", force, guiType == GuiPageType.Async);
    }

    private void HandleDevToolsStateChanged(BrowserComponent blazorGuiComponent, bool enabled)
    {
        _cefBlazorGuiService.ToggleDevTools(blazorGuiComponent.Entity.GetPlayer(), enabled);
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
