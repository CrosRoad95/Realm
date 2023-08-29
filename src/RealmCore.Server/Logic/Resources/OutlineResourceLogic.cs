namespace RealmCore.Server.Logic.Resources;

internal sealed class OutlineResourceLogic : ComponentLogic<OutlineComponent>
{
    private readonly IElementOutlineService _elementOutlineService;

    public OutlineResourceLogic(IEntityEngine entityEngine, IElementOutlineService elementOutlineService) : base(entityEngine)
    {
        _elementOutlineService = elementOutlineService;
    }

    protected override void ComponentAdded(OutlineComponent outlineComponent)
    {
        _elementOutlineService.SetElementOutline(outlineComponent.Entity.GetElement(), outlineComponent.Color);
    }

    protected override void ComponentDetached(OutlineComponent component)
    {
        _elementOutlineService.RemoveElementOutline(component.Entity.GetElement());
    }
}
