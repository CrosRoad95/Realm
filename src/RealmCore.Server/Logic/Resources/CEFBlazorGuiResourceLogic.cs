using RealmCore.Resources.CEFBlazorGui;

namespace RealmCore.Server.Logic.Resources;

internal class CEFBlazorGuiResourceLogic
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
        return null;
    }

    private void HandlePlayerBrowserReady(Player player)
    {
        if (_ecs.TryGetEntityByPlayer(player, out var entity))
        {
            var blazorGuiComponent = entity.AddComponent<BlazorGuiComponent>();
            blazorGuiComponent.InternalPathChanged = HandleInternalPathChanged;
        }
    }

    private void HandleInternalPathChanged(BlazorGuiComponent blazorGuiComponent, string? path, bool force)
    {
        var player = blazorGuiComponent.Entity.Player;
        _cefBlazorGuiService.SetPath(player, path ?? "", force);
    }
}
