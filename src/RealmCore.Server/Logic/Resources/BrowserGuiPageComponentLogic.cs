using RealmCore.Server.Components.Players.Abstractions;

namespace RealmCore.Server.Logic.Resources;

internal sealed class BrowserGuiPageComponentLogic : ComponentLogic<GuiBlazorComponent>
{
    private readonly IBrowserGuiService _browserGuiService;
    private readonly ILogger<BrowserGuiPageComponentLogic> _logger;

    public BrowserGuiPageComponentLogic(IEntityEngine entityEngine, IBrowserGuiService browserGuiService, ILogger<BrowserGuiPageComponentLogic> logger) : base(entityEngine)
    {
        _browserGuiService = browserGuiService;
        _logger = logger;
    }

    protected override void ComponentAdded(GuiBlazorComponent guiBlazorComponent)
    {
        using var _ = _logger.BeginEntity(guiBlazorComponent.Entity);
        if(_browserGuiService.TryGetKeyByEntity(guiBlazorComponent.Entity, out var key))
        {
            //var url = $"{guiBlazorComponent.Path}?{_browserGuiService.KeyName}={key}";
            var url = guiBlazorComponent.Path;
            guiBlazorComponent.Entity.GetRequiredComponent<BrowserComponent>().LoadRemotePage(url, true);
            //guiBlazorComponent.NavigationRequested += HandleNavigationRequested;

            _logger.LogInformation("Gui {guiPageType} opened", guiBlazorComponent.GetType().Name);
        }
        ;
    }

    private void HandleNavigationRequested(GuiBlazorComponent that, GuiBlazorComponent targetComponent)
    {
        var entity = that.Entity;
        entity.DestroyComponent(that);
        entity.AddComponent(targetComponent);
    }

    protected override void ComponentDetached(GuiBlazorComponent guiBlazorComponent)
    {
        var browserGuiComponent = guiBlazorComponent.Entity.GetRequiredComponent<BrowserComponent>();
        guiBlazorComponent.NavigationRequested -= HandleNavigationRequested;
        //browserGuiComponent.LoadRemotePage("/");
        browserGuiComponent.Visible = false;

        _logger.LogInformation("Gui {guiPageType} closed", guiBlazorComponent.GetType().Name);
    }
}
