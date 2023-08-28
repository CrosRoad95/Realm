namespace RealmCore.Server.Logic.Components;

internal class StatefulHudComponentLogic : ComponentLogic<IStatefulHudComponent>
{
    private readonly IOverlayService _overlayService;

    public StatefulHudComponentLogic(IEntityEngine entityEngine, IOverlayService overlayService) : base(entityEngine)
    {
        _overlayService = overlayService;
    }

    protected override void ComponentAdded(IStatefulHudComponent statefulHudComponent)
    {
        statefulHudComponent.BuildHud(_overlayService);
    }

    protected override void ComponentDetached(IStatefulHudComponent statefulHudComponent)
    {
        _overlayService.RemoveHud(statefulHudComponent.Entity.GetPlayer(), statefulHudComponent.Id);
    }
}
