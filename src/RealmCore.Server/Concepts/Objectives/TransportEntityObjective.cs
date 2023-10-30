namespace RealmCore.Server.Concepts.Objectives;

public class TransportEntityObjective : Objective
{
    private readonly Entity? _entity;
    private readonly Vector3 _position;
    private readonly bool _createMarker;
    private PlayerPrivateElementComponent<MarkerElementComponent>? _markerElementComponent;
    private PlayerPrivateElementComponent<CollisionSphereElementComponent> _collisionSphereElementComponent = default!;
    private Entity _playerEntity = default!;
    private IElementCollection _elementCollection = default!;
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

    protected override void Load(IServiceProvider serviceProvider, Entity playerEntity)
    {
        var entityFactory = serviceProvider.GetRequiredService<IEntityFactory>();
        _elementCollection = serviceProvider.GetRequiredService<IElementCollection>();
        _playerEntity = playerEntity;
        using var scopedEntityFactory = entityFactory.CreateScopedEntityFactory(playerEntity);
        if (_createMarker)
        {
            scopedEntityFactory.CreateMarker(MarkerType.Arrow, _position, Color.White);
            _markerElementComponent = scopedEntityFactory.GetLastCreatedComponent<PlayerPrivateElementComponent<MarkerElementComponent>>();
        }

        scopedEntityFactory.CreateCollisionSphere(_position, 1.5f);
        _collisionSphereElementComponent = scopedEntityFactory.GetLastCreatedComponent<PlayerPrivateElementComponent<CollisionSphereElementComponent>>();
        _collisionSphereElementComponent.ElementComponent.ElementEntered += HandleElementEntered;

    }

    private void HandleElementEntered(Element element)
    {
        var entity = element.TryUpCast();
        if (_entity == null || entity == null)
            return;

        if (entity.TryGetComponent(out OwnerComponent ownerComponent))
        {
            if (ownerComponent.OwningEntity == _playerEntity)
            {
                if (CheckEntity != null)
                {
                    if (CheckEntity(entity))
                    {
                        Complete(this, entity);
                        return;
                    }
                }
                Complete(this, entity);
            }
        }
    }

    public override void Update()
    {
        if (_entity == null)
        {
            var elements = _elementCollection.GetWithinRange(_collisionSphereElementComponent.ElementComponent.Position, _collisionSphereElementComponent.ElementComponent.Radius);
            foreach (var element in elements)
                _collisionSphereElementComponent.ElementComponent.CheckElementWithin(element);
        }
        else
        {
            if(_entity.TryGetElement(out var element))
            {
                if (_entity.TryGetComponent(out LiftableWorldObjectComponent liftableWorldObjectComponent))
                {
                    if (liftableWorldObjectComponent.Owner == null) // Accept only dropped entities.
                        _collisionSphereElementComponent.ElementComponent.CheckElementWithin(element);
                }
                else
                {
                    _collisionSphereElementComponent.ElementComponent.CheckElementWithin(element);
                }
            }
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
