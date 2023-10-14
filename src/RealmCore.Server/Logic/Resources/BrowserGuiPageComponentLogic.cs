using RealmCore.Server.Components.Players.Abstractions;

namespace RealmCore.Server.Logic.Resources;

internal sealed class BrowserGuiPageComponentLogic : ComponentLogic<BrowserGuiComponent>
{
    private readonly IBrowserGuiService _browserGuiService;
    private readonly ILogger<BrowserGuiPageComponentLogic> _logger;

    public BrowserGuiPageComponentLogic(IEntityEngine entityEngine, IBrowserGuiService browserGuiService, ILogger<BrowserGuiPageComponentLogic> logger) : base(entityEngine)
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
            browserGuiComponent.Entity.GetRequiredComponent<BrowserComponent>().Path = url;

            _logger.LogInformation("Gui {guiPageType} opened", browserGuiComponent.GetType().Name);
        }
    }

    protected override void ComponentDetached(BrowserGuiComponent browserGuiComponent)
    {
        var browserComponent = browserGuiComponent.Entity.GetRequiredComponent<BrowserComponent>();
        browserComponent.Visible = false;

        _logger.LogInformation("Gui {guiPageType} closed", browserGuiComponent.GetType().Name);
    }
}
