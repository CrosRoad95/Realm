namespace RealmCore.Server.Concepts.Objectives;

public class TransportEntityObjective : Objective
{
    private readonly Entity? _entity;
    private readonly Vector3 _position;
    private readonly bool _createMarker;
    private PlayerPrivateElementComponent<MarkerElementComponent>? _markerElementComponent;
    private PlayerPrivateElementComponent<CollisionSphereElementComponent> _collisionSphereElementComponent = default!;
    private Entity _playerEntity = default!;
    private System.Timers.Timer _checkEnteredTimer = default!;

    public override Vector3 Position => _position;

    public TransportEntityObjective(Entity? entity, Vector3 position, bool createMarker = true)
    {
        _entity = entity;
        _position = position;
        _createMarker = createMarker;
    }

    protected override void Load(IEntityFactory entityFactory, Entity playerEntity)
    {
        _playerEntity = playerEntity;
        if(_createMarker)
            _markerElementComponent = entityFactory.CreateMarkerFor(playerEntity, _position, MarkerType.Arrow, Color.White);
        _collisionSphereElementComponent = entityFactory.CreateCollisionSphereFor(playerEntity, _position, 1.5f);
        _collisionSphereElementComponent.ElementComponent.EntityEntered = EntityEntered;
        _checkEnteredTimer = new System.Timers.Timer(TimeSpan.FromSeconds(0.25f));
        _checkEnteredTimer.Elapsed += HandleElapsed;
        _checkEnteredTimer.Start();
    }

    private void HandleElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        try
        {
            ThrowIfDisposed();

            if(_entity == null)
            {
                _collisionSphereElementComponent.ElementComponent.RefreshColliders();
            }
            else
            {
                if (_entity.TryGetComponent(out LiftableWorldObjectComponent liftableWorldObjectComponent))
                {
                    if (liftableWorldObjectComponent.Owner == null) // Accept only dropped entities.
                        _collisionSphereElementComponent.ElementComponent.CheckCollisionWith(_entity);
                }
                else
                {
                    _collisionSphereElementComponent.ElementComponent.CheckCollisionWith(_entity);
                }
            }
        }
        catch (Exception)
        {
            // TODO: Capture
        }
    }

    private void EntityEntered(Entity colShapeEntity, Entity entity)
    {
        ThrowIfDisposed();

        if (_entity == null)
        {
            if(entity.TryGetComponent(out OwnerComponent ownerComponent))
            {
                if(ownerComponent.OwningEntity == _playerEntity)
                    Complete(this, entity);
            }
        }
        else
        {
            if (entity == _entity)
                Complete(this);
        }
    }

    public override void Dispose()
    {
        if(_entity != null)
            _entity.Dispose();
        _checkEnteredTimer.Stop();
        _checkEnteredTimer.Dispose();
        if(_markerElementComponent != null)
            _playerEntity.TryDestroyComponent(_markerElementComponent);
        _playerEntity.TryDestroyComponent(_collisionSphereElementComponent);
        base.Dispose();
    }
}
