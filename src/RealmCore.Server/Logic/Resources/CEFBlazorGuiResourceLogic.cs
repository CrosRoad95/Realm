using RealmCore.Resources.CEFBlazorGui;

namespace RealmCore.Server.Logic.Resources;

internal sealed class CEFBlazorGuiResourceLogic : ComponentLogic<BrowserGuiComponent>
{
    private readonly IECS _ecs;
    private readonly ICEFBlazorGuiService _cefBlazorGuiService;
    private readonly IBlazorGuiService _blazorGuiService;
    private readonly IOptions<BlazorOptions> _blazorOptions;
    private readonly ChatBox _chatBox;

    public CEFBlazorGuiResourceLogic(IECS ecs, ICEFBlazorGuiService cefBlazorGuiService, IBlazorGuiService blazorGuiService, IOptions<BlazorOptions> blazorOptions, ChatBox chatBox) : base(ecs)
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

    protected override void ComponentAdded(BrowserGuiComponent blazorGuiComponent)
    {
        blazorGuiComponent.DevToolsStateChanged += HandleDevToolsStateChanged;
        blazorGuiComponent.PathChanged += HandlePathChanged;
        blazorGuiComponent.VisibleChanged += HandleVisibleChanged;
        blazorGuiComponent.RemotePathChanged += HandleRemotePathChanged;
    }

    protected override void ComponentRemoved(BrowserGuiComponent blazorGuiComponent)
    {
        blazorGuiComponent.DevToolsStateChanged -= HandleDevToolsStateChanged;
        blazorGuiComponent.PathChanged -= HandlePathChanged;
        blazorGuiComponent.VisibleChanged -= HandleVisibleChanged;
        blazorGuiComponent.RemotePathChanged -= HandleRemotePathChanged;
    }

    private void HandleRemotePathChanged(BrowserGuiComponent blazorGuiComponent, string path)
    {
        _cefBlazorGuiService.SetRemotePath(blazorGuiComponent.Entity.Player, path);
    }

    private void HandleVisibleChanged(BrowserGuiComponent blazorGuiComponent, bool visible)
    {
        _cefBlazorGuiService.SetVisible(blazorGuiComponent.Entity.Player, visible);
    }

    private void HandlePathChanged(BrowserGuiComponent blazorGuiComponent, string? path, bool force, GuiPageType guiType, GuiPageChangeSource guiPageChangeSource)
    {
        if(guiPageChangeSource == GuiPageChangeSource.Server)
            _cefBlazorGuiService.SetPath(blazorGuiComponent.Entity.Player, path ?? "", force, guiType == GuiPageType.Async);
    }

    private void HandleDevToolsStateChanged(BrowserGuiComponent blazorGuiComponent, bool enabled)
    {
        _cefBlazorGuiService.ToggleDevTools(blazorGuiComponent.Entity.Player, enabled);
    }

    private async Task HandleInvokeVoidAsyncInvoked(Player player, string identifier, string args)
    {
        if(_ecs.TryGetEntityByPlayer(player, out var entity))
        {
            switch(identifier)
            {
                case "_ready":
                    if (!entity.HasComponent<BrowserGuiComponent>())
                        _cefBlazorGuiService.RelayPlayerBlazorReady?.Invoke(player);
                    break;
                case "_pageReady":
                    if(_blazorOptions.Value.Mode == CEFGuiBlazorMode.Remote)
                        _cefBlazorGuiService.SetVisible(player, true);
                    break;
                default:
                    if (entity.TryGetComponent(out BrowserGuiComponent component))
                        await _blazorGuiService.RelayInvokeVoidAsync(component, identifier, args);
                    break;
            }
        }
    }

    private Task<object?> HandleInvokeAsyncInvoked(Player player, string identifier, string args)
    {
        if(_ecs.TryGetEntityByPlayer(player, out var entity))
        {
            if (entity.TryGetComponent(out BrowserGuiComponent component))
                return _blazorGuiService.RelayInvokeAsync(component, identifier, args);
        }
        return Task.FromResult<object?>(null);
    }

    private void HandlePlayerBrowserReadyCore(Entity playerEntity)
    {
        if(!playerEntity.HasComponent<BrowserGuiComponent>())
            playerEntity.AddComponent<BrowserGuiComponent>();
    }

    private void HandlePlayerBrowserReady(Player player)
    {
        if (_ecs.TryGetEntityByPlayer(player, out var entity))
            HandlePlayerBrowserReadyCore(entity);
    }
}
