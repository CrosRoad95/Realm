namespace RealmCore.Server.Logic.Resources;

internal sealed class OutlineResourceLogic : ComponentLogic<OutlineComponent>
{
    private readonly IElementOutlineService _elementOutlineService;

    public OutlineResourceLogic(IElementFactory elementFactory, IElementOutlineService elementOutlineService) : base(elementFactory)
    {
        _elementOutlineService = elementOutlineService;
    }

    protected override void ComponentAdded(OutlineComponent outlineComponent)
    {
        _elementOutlineService.SetElementOutline(outlineComponent.Element, outlineComponent.Color);
    }

    protected override void ComponentDetached(OutlineComponent component)
    {
        _elementOutlineService.RemoveElementOutline(component.Element);
    }
}
