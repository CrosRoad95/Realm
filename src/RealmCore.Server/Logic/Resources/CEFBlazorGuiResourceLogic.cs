using RealmCore.Resources.CEFBlazorGui;

namespace RealmCore.Server.Logic.Resources;

internal sealed class CEFBlazorGuiResourceLogic : ComponentLogic<BlazorGuiComponent>
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
        _cefBlazorGuiService.RelayPlayerBlazorReady = HandlePlayerBrowserReady;
        if(blazorOptions.Value.Mode == CEFGuiBlazorMode.Dev)
            _ecs.EntityCreated += HandleEntityCreated;
    }

    private void HandleEntityCreated(Entity entity)
    {
        if(entity.Tag == EntityTag.Player)
            HandlePlayerBrowserReadyCore(entity);
    }

    protected override void ComponentAdded(BlazorGuiComponent blazorGuiComponent)
    {
        blazorGuiComponent.DevToolsStateChanged += HandleDevToolsStateChanged;
        blazorGuiComponent.PathChanged += HandlePathChanged;
        blazorGuiComponent.VisibleChanged += HandleVisibleChanged;
    }

    protected override void ComponentRemoved(BlazorGuiComponent blazorGuiComponent)
    {
        blazorGuiComponent.DevToolsStateChanged -= HandleDevToolsStateChanged;
        blazorGuiComponent.PathChanged -= HandlePathChanged;
        blazorGuiComponent.VisibleChanged -= HandleVisibleChanged;
    }

    private void HandleVisibleChanged(BlazorGuiComponent blazorGuiComponent, bool visible)
    {
        _cefBlazorGuiService.SetVisible(blazorGuiComponent.Entity.Player, visible);
    }

    private void HandlePathChanged(BlazorGuiComponent blazorGuiComponent, string? path, bool force, GuiPageType guiType, GuiPageChangeSource guiPageChangeSource)
    {
        if(guiPageChangeSource == GuiPageChangeSource.Server)
            _cefBlazorGuiService.SetPath(blazorGuiComponent.Entity.Player, path ?? "", force, guiType == GuiPageType.Async);
    }

    private void HandleDevToolsStateChanged(BlazorGuiComponent blazorGuiComponent, bool enabled)
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
                    if (!entity.HasComponent<BlazorGuiComponent>())
                        _cefBlazorGuiService.RelayPlayerBlazorReady?.Invoke(player);
                    break;
                case "_pageReady":
                    if(_blazorOptions.Value.Mode == CEFGuiBlazorMode.Dev)
                        _cefBlazorGuiService.SetVisible(player, true);
                    break;
                default:
                    if (entity.TryGetComponent(out BlazorGuiComponent component))
                        await _blazorGuiService.RelayInvokeVoidAsync(component, identifier, args);
                    break;
            }
        }
    }

    private Task<object?> HandleInvokeAsyncInvoked(Player player, string identifier, string args)
    {
        if(_ecs.TryGetEntityByPlayer(player, out var entity))
        {
            if (entity.TryGetComponent(out BlazorGuiComponent component))
                return _blazorGuiService.RelayInvokeAsync(component, identifier, args);
        }
        return Task.FromResult<object?>(null);
    }

    private void HandlePlayerBrowserReadyCore(Entity playerEntity)
    {
        if (_blazorOptions.Value.Mode == CEFGuiBlazorMode.Dev)
            _chatBox.OutputTo(playerEntity, "Aktywowano tryb developerski dla blazor gui.");

        if(!playerEntity.HasComponent<BlazorGuiComponent>())
            playerEntity.AddComponent<BlazorGuiComponent>();
    }

    private void HandlePlayerBrowserReady(Player player)
    {
        if (_ecs.TryGetEntityByPlayer(player, out var entity))
            HandlePlayerBrowserReadyCore(entity);
    }
}
