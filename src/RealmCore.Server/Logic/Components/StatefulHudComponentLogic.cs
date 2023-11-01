namespace RealmCore.Server.Logic.Components;

internal sealed class StatefulHudComponentLogic : ComponentLogic<IStatefulHudComponent>
{
    private readonly IOverlayService _overlayService;

    public StatefulHudComponentLogic(IElementFactory elementFactory, IOverlayService overlayService) : base(elementFactory)
    {
        _overlayService = overlayService;
    }

    protected override void ComponentAdded(IStatefulHudComponent statefulHudComponent)
    {
        statefulHudComponent.BuildHud(_overlayService);
    }

    protected override void ComponentDetached(IStatefulHudComponent statefulHudComponent)
    {
        _overlayService.RemoveHud((RealmPlayer)statefulHudComponent.Element, statefulHudComponent.Id);
    }
}
