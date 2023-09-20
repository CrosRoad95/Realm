namespace RealmCore.Server.Concepts.Objectives;

public class TransportEntityObjective : Objective
{
    private readonly Entity? _entity;
    private readonly Vector3 _position;
    private readonly bool _createMarker;
    private PlayerPrivateElementComponent<MarkerElementComponent>? _markerElementComponent;
    private PlayerPrivateElementComponent<CollisionSphereElementComponent> _collisionSphereElementComponent = default!;
    private Entity _playerEntity = default!;
    public Func<Entity, bool>? CheckEntity { get; set; }

    public override Vector3 Position => _position;

    public TransportEntityObjective(Entity entity, Vector3 position, bool createMarker = true)
    {
        _entity = entity;
        _position = position;
        _createMarker = createMarker;

        _entity.Disposed += HandleDisposed;
    }

    private void HandleDisposed(Entity entity)
    {
        if(_entity != null)
        {
            _entity.Disposed -= HandleDisposed;
            Incomplete(this);
        }
    }

    public TransportEntityObjective(Vector3 position, bool createMarker = true)
    {
        _position = position;
        _createMarker = createMarker;
    }

    protected override void Load(IEntityFactory entityFactory, Entity playerEntity)
    {
        _playerEntity = playerEntity;
        using var scopedEntityFactory = entityFactory.CreateScopedEntityFactory(playerEntity);
        if (_createMarker)
        {
            scopedEntityFactory.CreateMarker(MarkerType.Arrow, _position, Color.White);
            _markerElementComponent = scopedEntityFactory.GetLastCreatedComponent<PlayerPrivateElementComponent<MarkerElementComponent>>();
        }

        scopedEntityFactory.CreateCollisionSphere(_position, 1.5f);
        _collisionSphereElementComponent = scopedEntityFactory.GetLastCreatedComponent<PlayerPrivateElementComponent<CollisionSphereElementComponent>>();
        _collisionSphereElementComponent.ElementComponent.EntityEntered = EntityEntered;

    }

    public override void Update()
    {
        ThrowIfDisposed();

        if (_entity == null)
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

    private void EntityEntered(Entity colShapeEntity, Entity entity)
    {
        ThrowIfDisposed();

        if (_entity == null)
        {
            if(entity.TryGetComponent(out OwnerComponent ownerComponent))
            {
                if(ownerComponent.OwningEntity == _playerEntity)
                {
                    if(CheckEntity != null)
                    {
                        if(CheckEntity(entity))
                        {
                            Complete(this, entity);
                            return;
                        }
                    }
                    Complete(this, entity);
                }
            }
        }
        else
        {
            _entity.Disposed -= HandleDisposed;
            if (entity == _entity)
                Complete(this, entity);
        }
    }

    public override void Dispose()
    {
        if(_markerElementComponent != null)
            _playerEntity.TryDestroyComponent(_markerElementComponent);
        _playerEntity.TryDestroyComponent(_collisionSphereElementComponent);
        base.Dispose();
    }
}
