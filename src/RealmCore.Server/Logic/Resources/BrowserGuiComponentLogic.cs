using RealmCore.Server.Components.Players.Abstractions;

namespace RealmCore.Server.Logic.Resources;

internal sealed class BrowserGuiComponentLogic : ComponentLogic<BrowserGuiComponent>
{
    private readonly IBrowserGuiService _browserGuiService;
    private readonly ILogger<BrowserGuiComponentLogic> _logger;

    public BrowserGuiComponentLogic(IElementFactory elementFactory, IBrowserGuiService browserGuiService, ILogger<BrowserGuiComponentLogic> logger) : base(elementFactory)
    {
        _browserGuiService = browserGuiService;
        _logger = logger;
    }

    protected override void ComponentAdded(BrowserGuiComponent browserGuiComponent)
    {
        using var _ = _logger.BeginElement(browserGuiComponent.Element);
        var player = (RealmPlayer)browserGuiComponent.Element;
        if (_browserGuiService.TryGetKeyByPlayer(player, out var key))
        {
            var url = browserGuiComponent.Path;
            var browserService = player.GetRequiredService<IRealmBrowserService>();
            browserService.Open(url);
            _logger.LogInformation("Gui {guiPageType} opened", browserGuiComponent.GetType().Name);
        }
        browserGuiComponent.NavigationRequested += HandleNavigationRequested;
    }

    protected override void ComponentDetached(BrowserGuiComponent browserGuiComponent)
    {
        browserGuiComponent.NavigationRequested -= HandleNavigationRequested;
        var player = (RealmPlayer)browserGuiComponent.Element;
        var browserService = player.GetRequiredService<IRealmBrowserService>();
        browserService.SetPath("/realmEmpty");
        browserService.Close();

        _logger.LogInformation("Gui {guiPageType} closed", browserGuiComponent.GetType().Name);
    }

    private void HandleNavigationRequested(BrowserGuiComponent browserGuiComponent, BrowserGuiComponent targetGuiComponent)
    {
        var player = (RealmPlayer)browserGuiComponent.Element;
        player.DestroyComponent(browserGuiComponent);
        player.AddComponent(targetGuiComponent);
    }
}
