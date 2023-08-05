using RealmCore.Resources.CEFBlazorGui;

namespace RealmCore.Server.Logic.Resources;

internal sealed class CEFBlazorGuiResourceLogic
{
    private readonly IECS _ecs;
    private readonly ICEFBlazorGuiService _cefBlazorGuiService;
    private readonly IBlazorGuiService _blazorGuiService;

    public CEFBlazorGuiResourceLogic(IECS ecs, ICEFBlazorGuiService cefBlazorGuiService, IBlazorGuiService blazorGuiService)
    {
        _ecs = ecs;
        _cefBlazorGuiService = cefBlazorGuiService;
        _blazorGuiService = blazorGuiService;

        _cefBlazorGuiService.RelayVoidAsyncInvoked = HandleInvokeVoidAsyncInvoked;
        _cefBlazorGuiService.RelayAsyncInvoked = HandleInvokeAsyncInvoked;
        _cefBlazorGuiService.RelayPlayerBlazorReady = HandlePlayerBrowserReady;
    }

    private async Task HandleInvokeVoidAsyncInvoked(Player player, string identifier, string args)
    {
        if(_ecs.TryGetEntityByPlayer(player, out var entity))
        {
            if (entity.TryGetComponent(out BlazorGuiComponent component))
                await _blazorGuiService.RelayInvokeVoidAsync(component, identifier, args);
        }
    }

    private Task<object> HandleInvokeAsyncInvoked(Player player, string identifier, string args)
    {
        if(_ecs.TryGetEntityByPlayer(player, out var entity))
        {
            if (entity.TryGetComponent(out BlazorGuiComponent component))
                return _blazorGuiService.RelayInvokeAsync(component, identifier, args);
        }
        return Task.FromResult<object>(null);
    }

    private void HandlePlayerBrowserReady(Player player)
    {
        if (_ecs.TryGetEntityByPlayer(player, out var entity))
        {
            var blazorGuiComponent = new BlazorGuiComponent();
            blazorGuiComponent.InternalPathChanged = HandleInternalPathChanged;
            entity.AddComponent(blazorGuiComponent);
        }
    }

    private void HandleInternalPathChanged(BlazorGuiComponent blazorGuiComponent, string? path, bool force, bool isAsync)
    {
        var player = blazorGuiComponent.Entity.Player;
        _cefBlazorGuiService.SetPath(player, path ?? "", force, isAsync);
    }
}
