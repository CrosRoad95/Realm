namespace RealmCore.Server.Logic.Components;

internal sealed class Hud3dComponentBaseLogic : ComponentLogic<Hud3dComponentBase>
{
    private readonly IOverlayService _overlayService;

    public Hud3dComponentBaseLogic(IElementFactory elementFactory, IOverlayService overlayService) : base(elementFactory)
    {
        _overlayService = overlayService;
    }
    //OverlayService.RemoveHud3d(_id.ToString());

    protected override void ComponentAdded(Hud3dComponentBase hud3dComponentBase)
    {
        hud3dComponentBase.Removed = HandleRemoved;
        hud3dComponentBase.StateChanged = HandleStateChanged;
    }

    protected override void ComponentDetached(Hud3dComponentBase hud3dComponentBase)
    {
        hud3dComponentBase.Removed = null;
        hud3dComponentBase.StateChanged = null;
    }

    private void HandleStateChanged(Hud3dComponentBase hud3dComponentBase, int id, Dictionary<int, object?> state)
    {
        hud3dComponentBase.Removed = null;
        _overlayService.SetHud3dState(id.ToString(), state);
    }

    private void HandleRemoved(Hud3dComponentBase hud3DComponentBase, int id)
    {
        _overlayService.RemoveHud3d(id.ToString());
    }
}
