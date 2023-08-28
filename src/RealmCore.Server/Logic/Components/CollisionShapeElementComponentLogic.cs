using RealmCore.ECS;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements.ColShapes;

namespace RealmCore.Server.Logic.Components;

internal sealed class CollisionShapeElementComponentLogic : ComponentLogic<CollisionShapeElementComponent>
{
    private readonly IEntityEngine _entityEngine;
    private readonly IElementCollection _elementCollection;

    public CollisionShapeElementComponentLogic(IEntityEngine entityEngine, IElementCollection elementCollection) : base(entityEngine)
    {
        _entityEngine = entityEngine;
        _elementCollection = elementCollection;
    }

    protected override void ComponentAdded(CollisionShapeElementComponent collisionShapeElementComponent)
    {
        collisionShapeElementComponent.CollidersRefreshed = HandleCollidersRefreshed;
    }

    protected override void ComponentDetached(CollisionShapeElementComponent collisionShapeElementComponent)
    {
        collisionShapeElementComponent.CollidersRefreshed = null;
    }

    private void HandleCollidersRefreshed(CollisionShapeElementComponent collisionShapeElementComponent)
    {
        var element = collisionShapeElementComponent.Element;
        if (element is CollisionSphere collisionSphere)
        {
            var elements = _elementCollection.GetWithinRange(collisionSphere.Position, collisionSphere.Radius);
            foreach (var element2 in elements)
            {
                if (_entityEngine.TryGetByElement(element2, out Entity entity))
                    collisionShapeElementComponent.CheckCollisionWith(entity);
            }
        }
    }
}
