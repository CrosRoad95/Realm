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
        ecs.EntityCreated += HandleEntityCreated;

        _cefBlazorGuiService.InvokeVoidAsyncInvoked = HandleInvokeVoidAsyncInvoked;
        _cefBlazorGuiService.InvokeAsyncInvoked = HandleInvokeAsyncInvoked;
    }

    private void HandleInvokeVoidAsyncInvoked(Player player, string identifier, string args)
    {
        if(_ecs.TryGetEntityByPlayer(player, out var entity))
        {
            if (entity.TryGetComponent(out BlazorGuiComponent component))
                _blazorGuiService.RelayInvokeVoidAsync(component, identifier, args);

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

    private void HandleEntityCreated(Entity entity)
    {
        if(entity.Tag != EntityTag.Player)
            return;

        var blazorGuiComponent = entity.AddComponent<BlazorGuiComponent>();
        blazorGuiComponent.InternalPathChanged += HandleInternalPathChanged;
    }

    private void HandleInternalPathChanged(BlazorGuiComponent blazorGuiComponent, string? path)
    {
        var player = blazorGuiComponent.Entity.Player;
        _cefBlazorGuiService.SetPath(player, path ?? "");
    }
}
