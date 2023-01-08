namespace Realm.Domain.Concepts.Objectives;

public class MarkerEnterObjective : Objective
{
    private readonly Entity _markerEntity;
    private readonly Entity _collisionShapeEntity;

    public MarkerEnterObjective(Entity markerEntity, Entity collisionShapeEntity)
    {
        _markerEntity = markerEntity;
        _collisionShapeEntity = collisionShapeEntity;
        _collisionShapeEntity.Transform.Position = _markerEntity.Transform.Position;
        _collisionShapeEntity.GetRequiredComponent<CollisionSphereElementComponent>().EntityEntered += EntityEntered;
    }

    private void EntityEntered(Entity entity)
    {
        if(entity == Entity)
            Complete();
    }

    public override async ValueTask DisposeAsync()
    {
        await _markerEntity.Destroy();
        await _collisionShapeEntity.Destroy();
    }
}
