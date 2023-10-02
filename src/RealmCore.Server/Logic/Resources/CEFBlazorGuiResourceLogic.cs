using RealmCore.Resources.CEFBlazorGui;

namespace RealmCore.Server.Logic.Resources;

internal sealed class CEFBlazorGuiResourceLogic : ComponentLogic<BrowserComponent>
{
    private readonly IEntityEngine _ecs;
    private readonly ICEFBlazorGuiService _cefBlazorGuiService;
    private readonly IBlazorGuiService _blazorGuiService;
    private readonly IOptions<BrowserOptions> _blazorOptions;
    private readonly ChatBox _chatBox;

    public CEFBlazorGuiResourceLogic(IEntityEngine ecs, ICEFBlazorGuiService cefBlazorGuiService, IBlazorGuiService blazorGuiService, IOptions<BrowserOptions> blazorOptions, ChatBox chatBox) : base(ecs)
    {
        _ecs = ecs;
        _cefBlazorGuiService = cefBlazorGuiService;
        _blazorGuiService = blazorGuiService;
        _blazorOptions = blazorOptions;
        _chatBox = chatBox;
        _cefBlazorGuiService.RelayVoidAsyncInvoked = HandleInvokeVoidAsyncInvoked;
        _cefBlazorGuiService.RelayAsyncInvoked = HandleInvokeAsyncInvoked;
        _cefBlazorGuiService.RelayPlayerBrowserReady = HandlePlayerBrowserReady;
    }

    protected override void ComponentAdded(BrowserComponent blazorGuiComponent)
    {
        blazorGuiComponent.DevToolsStateChanged += HandleDevToolsStateChanged;
        blazorGuiComponent.PathChanged += HandlePathChanged;
        blazorGuiComponent.VisibleChanged += HandleVisibleChanged;
        blazorGuiComponent.RemotePathChanged += HandleRemotePathChanged;
    }

    protected override void ComponentDetached(BrowserComponent blazorGuiComponent)
    {
        blazorGuiComponent.DevToolsStateChanged -= HandleDevToolsStateChanged;
        blazorGuiComponent.PathChanged -= HandlePathChanged;
        blazorGuiComponent.VisibleChanged -= HandleVisibleChanged;
        blazorGuiComponent.RemotePathChanged -= HandleRemotePathChanged;
    }

    private void HandleRemotePathChanged(BrowserComponent blazorGuiComponent, string path)
    {
        _cefBlazorGuiService.SetRemotePath(blazorGuiComponent.Entity.GetPlayer(), path);
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

    private async Task HandleInvokeVoidAsyncInvoked(Player player, string identifier, string args)
    {
        if(_ecs.TryGetEntityByPlayer(player, out var entity))
        {
            switch(identifier)
            {
                case "_ready":
                    if (!entity.HasComponent<BrowserComponent>())
                        _cefBlazorGuiService.RelayPlayerBlazorReady?.Invoke(player);
                    break;
                case "_pageReady":
                    if(_blazorOptions.Value.Mode == CEFGuiBlazorMode.Remote)
                        _cefBlazorGuiService.SetVisible(player, true);
                    break;
                default:
                    if (entity.TryGetComponent(out BrowserComponent component))
                        await _blazorGuiService.RelayInvokeVoidAsync(component, identifier, args);
                    break;
            }
        }
    }

    private Task<object?> HandleInvokeAsyncInvoked(Player player, string identifier, string args)
    {
        if(_ecs.TryGetEntityByPlayer(player, out var entity))
        {
            if (entity.TryGetComponent(out BrowserComponent component))
                return _blazorGuiService.RelayInvokeAsync(component, identifier, args);
        }
        return Task.FromResult<object?>(null);
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
