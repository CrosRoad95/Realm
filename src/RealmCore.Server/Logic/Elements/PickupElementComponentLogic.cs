namespace RealmCore.Server.Logic.Elements;

internal sealed class PickupElementComponentLogic : ComponentLogic<PickupElementComponent>
{
    private readonly IEntityEngine _entityEngine;
    private readonly ILogger<PickupElementComponentLogic> _logger;

    public PickupElementComponentLogic(IEntityEngine entityEngine, ILogger<PickupElementComponentLogic> logger) : base(entityEngine)
    {
        _entityEngine = entityEngine;
        _logger = logger;
    }

    protected override void ComponentAdded(PickupElementComponent pickupElementComponent)
    {
        pickupElementComponent.ElementEntered += HandleElementEntered;
        pickupElementComponent.ElementLeft += HandleElementLeft;
    }

    private void HandleElementEntered(PickupElementComponent pickupElementComponent, Element element)
    {
        if (pickupElementComponent.EntityEntered == null)
            return;

        if (element is not IElementComponent elementComponent)
            return;
        var entity = elementComponent.Entity;

        var tag = entity.GetRequiredComponent<TagComponent>();
        if (tag is not PlayerTagComponent or VehicleTagComponent)
            return;

        try
        {
            if (!pickupElementComponent.CheckRules(entity))
                return;
        }
        catch (Exception ex)
        {
            _logger.LogHandleError(ex);
            return;
        }

        try
        {
            pickupElementComponent.EntityEntered(pickupElementComponent.Entity, entity);
        }
        catch (Exception ex)
        {
            _logger.LogHandleError(ex);
            return;
        }
    }

    private void HandleElementLeft(PickupElementComponent pickupElementComponent, Element element)
    {
        if (pickupElementComponent.EntityLeft == null)
            return;

        if (element is not IElementComponent elementComponent)
            return;
        var entity = elementComponent.Entity;

        var tag = entity.GetRequiredComponent<TagComponent>();
        if (tag is not PlayerTagComponent or VehicleTagComponent)
            return;

        try
        {
            if (!pickupElementComponent.CheckRules(entity))
                return;
        }
        catch (Exception ex)
        {
            _logger.LogHandleError(ex);
            return;
        }

        try
        {
            pickupElementComponent.EntityLeft(pickupElementComponent.Entity, entity);
        }
        catch (Exception ex)
        {
            _logger.LogHandleError(ex);
            return;
        }
    }
}
