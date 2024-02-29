namespace RealmCore.Server.Modules.Players.Gui.Dx;

internal sealed class PlayerHudLogic : PlayerLifecycle
{
    private readonly IOverlayService _overlayService;

    public PlayerHudLogic(MtaServer server, IOverlayService overlayService) : base(server)
    {
        _overlayService = overlayService;
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        player.Hud.LayerCreated += HandleLayerCreated;
        player.Hud.LayerRemoved += HandleLayerRemoved;
    }

    protected override void PlayerLeft(RealmPlayer player)
    {
        player.Hud.LayerCreated -= HandleLayerCreated;
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
