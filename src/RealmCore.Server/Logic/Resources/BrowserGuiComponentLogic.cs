using RealmCore.Server.Components.Players.Abstractions;

namespace RealmCore.Server.Logic.Resources;

internal sealed class BrowserGuiComponentLogic : ComponentLogic<BrowserGuiComponent>
{
    private readonly IBrowserGuiService _browserGuiService;
    private readonly ILogger<BrowserGuiComponentLogic> _logger;

    public BrowserGuiComponentLogic(IEntityEngine entityEngine, IBrowserGuiService browserGuiService, ILogger<BrowserGuiComponentLogic> logger) : base(entityEngine)
    {
        _browserGuiService = browserGuiService;
        _logger = logger;
    }

    protected override void ComponentAdded(BrowserGuiComponent browserGuiComponent)
    {
        using var _ = _logger.BeginEntity(browserGuiComponent.Entity);
        if(_browserGuiService.TryGetKeyByEntity(browserGuiComponent.Entity, out var key))
        {
            var url = browserGuiComponent.Path;
            var browserComponent = browserGuiComponent.Entity.GetRequiredComponent<BrowserComponent>();
            browserComponent.Path = url;
            browserComponent.Visible = true;
            _logger.LogInformation("Gui {guiPageType} opened", browserGuiComponent.GetType().Name);
        }
        browserGuiComponent.NavigationRequested += HandleNavigationRequested;
    }

    protected override void ComponentDetached(BrowserGuiComponent browserGuiComponent)
    {
        browserGuiComponent.NavigationRequested -= HandleNavigationRequested;
        var browserComponent = browserGuiComponent.Entity.GetRequiredComponent<BrowserComponent>();
        browserComponent.Path = "/realmEmpty";
        browserComponent.Visible = false;

        _logger.LogInformation("Gui {guiPageType} closed", browserGuiComponent.GetType().Name);
    }

    private void HandleNavigationRequested(BrowserGuiComponent browserGuiComponent, BrowserGuiComponent targetGuiComponent)
    {
        var entity = browserGuiComponent.Entity;
        entity.TryDestroyComponent<BrowserGuiComponent>();
        entity.AddComponent(targetGuiComponent);
    }
}
