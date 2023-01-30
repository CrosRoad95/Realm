using Realm.Domain.Components.CollisionShapes;

namespace Realm.Domain.Concepts.Objectives;

public class MarkerEnterObjective : Objective
{
    private readonly Vector3 _position;
    
    private MarkerElementComponent _markerElementComponent = default!;
    private CollisionSphereElementComponent _collisionSphereElementComponent = default!;
    private Entity _playerEntity = default!;
    private System.Timers.Timer _checkEnteredTimer = default!;

    public override Vector3 Position => _position;

    public MarkerEnterObjective(Vector3 position)
    {
        _position = position;
    }

    public override void Load(IEntityFactory entityFactory, Entity playerEntity)
    {
        _playerEntity = playerEntity;
        _markerElementComponent = entityFactory.CreateMarkerFor(playerEntity, _position, MarkerType.Arrow, Color.White);
        _collisionSphereElementComponent = entityFactory.CreateCollisionSphereFor(playerEntity, _position, 2);
        _collisionSphereElementComponent.EntityEntered = EntityEntered;
        _checkEnteredTimer = new System.Timers.Timer(TimeSpan.FromSeconds(0.25f));
        _checkEnteredTimer.Elapsed += HandleElapsed;
        _checkEnteredTimer.Start();
    }

    private void HandleElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        _collisionSphereElementComponent.CheckCollisionWith(_playerEntity);
    }

    private void EntityEntered(Entity entity)
    {
        if(entity == Entity)
            Complete();
    }

    public override void Dispose()
    {
        _checkEnteredTimer.Dispose();
        _collisionSphereElementComponent.EntityEntered = null;
        _playerEntity.TryDestroyComponent(_markerElementComponent);
        _playerEntity.TryDestroyComponent(_collisionSphereElementComponent);
        base.Dispose();
    }
}
