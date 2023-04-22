namespace RealmCore.Server.Logic.Resources;

internal sealed class OutlineResourceLogic
{
    private readonly ILogger<ClientInterfaceResourceLogic> _logger;
    private readonly IElementOutlineService _elementOutlineService;
    private readonly IECS _ecs;

    public OutlineResourceLogic(IElementOutlineService elementOutlineService, ILogger<ClientInterfaceResourceLogic> logger, IECS ecs)
    {
        _elementOutlineService = elementOutlineService;
        _ecs = ecs;
        _logger = logger;

        _ecs.EntityCreated += HandleEntityCreated;
    }


    private void HandleEntityCreated(Entity entity)
    {
        entity.Disposed += HandleEntityDestroyed;
        entity.ComponentAdded += HandleComponentAdded;
    }

    private void HandleEntityDestroyed(Entity entity)
    {
        entity.ComponentAdded -= HandleComponentAdded;
    }

    private void HandleComponentAdded(Component component)
    {
        if (component is OutlineComponent outlineComponent)
        {
            outlineComponent.Disposed += HandleOutlineComponentDisposed;
            _elementOutlineService.SetElementOutline(outlineComponent.Entity.Element, outlineComponent.Color);
        }
    }

    private void HandleOutlineComponentDisposed(Component component)
    {
        component.Disposed -= HandleOutlineComponentDisposed;
        _elementOutlineService.RemoveElementOutline(component.Entity.Element);
    }
}
