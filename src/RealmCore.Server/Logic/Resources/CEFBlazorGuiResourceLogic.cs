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

    private void HandleInvokeVoidAsyncInvoked(Player player, string kind, string identifier, string args)
    {
        if(_ecs.TryGetEntityByPlayer(player, out var entity))
        {
            if (entity.TryGetComponent(out BlazorGuiComponent component))
                _blazorGuiService.RelayInvokeVoidAsync(component, kind, identifier, args);
        }
    }

    private Task<object> HandleInvokeAsyncInvoked(Player player, string kind, string identifier, string args)
    {
        if(_ecs.TryGetEntityByPlayer(player, out var entity))
        {
            if (entity.TryGetComponent(out BlazorGuiComponent component))
                return _blazorGuiService.RelayInvokeAsync(component, kind, identifier, args);
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

    private void HandleInternalPathChanged(BlazorGuiComponent blazorGuiComponent, string? path)
    {
        var player = blazorGuiComponent.Entity.Player;
        _cefBlazorGuiService.SetPath(player, path ?? "");
    }
}
