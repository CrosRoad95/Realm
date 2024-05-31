namespace RealmCore.Server.Modules.Players.Gui.Dx;

internal sealed class PlayerHudHostedService : PlayerLifecycle, IHostedService
{
    private readonly IOverlayService _overlayService;

    public PlayerHudHostedService(PlayersEventManager playersEventManager, IOverlayService overlayService) : base(playersEventManager)
    {
        _overlayService = overlayService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        player.Hud.LayerAdded += HandleLayerCreated;
        player.Hud.LayerRemoved += HandleLayerRemoved;
    }

    protected override void PlayerLeft(RealmPlayer player)
    {
        player.Hud.LayerAdded -= HandleLayerCreated;
        player.Hud.LayerRemoved -= HandleLayerRemoved;
    }

    private void HandleLayerCreated(IPlayerHudFeature hudService, IHudLayer hudLayer)
    {
        hudLayer.BuildHud(_overlayService, hudService.Player);
    }

    private void HandleLayerRemoved(IPlayerHudFeature hudService, IHudLayer hudLayer)
    {
        _overlayService.RemoveHudLayer(hudService.Player, hudLayer.Id);
    }
}
