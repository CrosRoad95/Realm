using Realm.Domain.Components.CollisionShapes;
using Realm.Domain.Components.Object;

namespace Realm.Domain.Concepts.Objectives;

public class TransportEntityObjective : Objective
{
    private readonly Entity _entity;
    private readonly Vector3 _position;
    
    private MarkerElementComponent _markerElementComponent = default!;
    private CollisionSphereElementComponent _collisionSphereElementComponent = default!;
    private Entity _playerEntity = default!;
    private System.Timers.Timer _checkEnteredTimer = default!;

    public override Vector3 Position => _position;

    public TransportEntityObjective(Entity entity, Vector3 position)
    {
        _entity = entity;
        _position = position;
    }

    protected override void Load(IEntityFactory entityFactory, Entity playerEntity)
    {
        _playerEntity = playerEntity;
        _markerElementComponent = entityFactory.CreateMarkerFor(playerEntity, _position, MarkerType.Arrow, Color.White);
        _collisionSphereElementComponent = entityFactory.CreateCollisionSphereFor(playerEntity, _position, 1.5f);
        _collisionSphereElementComponent.EntityEntered = EntityEntered;
        _checkEnteredTimer = new System.Timers.Timer(TimeSpan.FromSeconds(0.25f));
        _checkEnteredTimer.Elapsed += HandleElapsed;
        _checkEnteredTimer.Start();
    }

    private void HandleElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        try
        {
            ThrowIfDisposed();

            if (_entity.TryGetComponent(out LiftableWorldObjectComponent liftableWorldObjectComponent))
            {
                if(liftableWorldObjectComponent.Owner == null) // Accept only dropped entities.
                    _collisionSphereElementComponent.CheckCollisionWith(_entity);
            }
            else
            {
                _collisionSphereElementComponent.CheckCollisionWith(_entity);
            }
        }
        catch(Exception)
        {
            // TODO: Capture
        }
    }

    private void EntityEntered(Entity entity)
    {
        ThrowIfDisposed();

        if (entity == _entity)
            Complete();
    }

    public override void Dispose()
    {
        _entity.Dispose();
        _checkEnteredTimer.Dispose();
        _collisionSphereElementComponent.EntityEntered = null;
        _playerEntity.TryDestroyComponent(_markerElementComponent);
        _playerEntity.TryDestroyComponent(_collisionSphereElementComponent);
        base.Dispose();
    }
}
