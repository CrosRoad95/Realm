namespace RealmCore.Server.Logic.Components;

internal sealed class CollisionShapeElementComponentLogic : ComponentLogic<ICollisionShape, PlayerPrivateElementComponentBase>
{
    private readonly IEntityEngine _entityEngine;
    private readonly IElementCollection _elementCollection;

    public CollisionShapeElementComponentLogic(IEntityEngine entityEngine, IElementCollection elementCollection) : base(entityEngine)
    {
        _entityEngine = entityEngine;
        _elementCollection = elementCollection;
    }

    protected override void ComponentAdded(ICollisionShape collisionShapeElementComponent)
    {

    }

    protected override void ComponentDetached(ICollisionShape collisionShapeElementComponent)
    {

    }

    protected override void ComponentAdded(PlayerPrivateElementComponentBase playerPrivateElementComponentBase)
    {
        if (playerPrivateElementComponentBase is PlayerPrivateElementComponent<CollisionSphereElementComponent> privateCollisionSphereElementComponent)
        {
            ComponentAdded(privateCollisionSphereElementComponent.ElementComponent);
        }
    }

    protected override void ComponentDetached(PlayerPrivateElementComponentBase playerPrivateElementComponentBase)
    {
        if (playerPrivateElementComponentBase is PlayerPrivateElementComponent<CollisionSphereElementComponent> privateCollisionSphereElementComponent)
        {
            ComponentDetached(privateCollisionSphereElementComponent.ElementComponent);
        }
    }
}
