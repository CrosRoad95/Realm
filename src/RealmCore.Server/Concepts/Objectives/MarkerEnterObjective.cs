namespace RealmCore.Server.Concepts.Objectives;

public class MarkerEnterObjective : Objective
{
    private readonly Vector3 _position;

    private PlayerPrivateElementComponent<MarkerElementComponent> _markerElementComponent = default!;
    private PlayerPrivateElementComponent<CollisionSphereElementComponent> _collisionSphereElementComponent = default!;
    private Entity _playerEntity = default!;

    public override Vector3 Position => _position;

    public MarkerEnterObjective(Vector3 position)
    {
        _position = position;
    }

    protected override void Load(IEntityFactory entityFactory, Entity playerEntity)
    {
        _playerEntity = playerEntity;
        using var scopedEntityFactory = entityFactory.CreateScopedEntityFactory(playerEntity);
        scopedEntityFactory.CreateMarker(MarkerType.Arrow, _position, Color.White);
        _markerElementComponent = scopedEntityFactory.GetLastCreatedComponent<PlayerPrivateElementComponent<MarkerElementComponent>>();
        scopedEntityFactory.CreateCollisionSphere(_position, 2);
        _collisionSphereElementComponent = scopedEntityFactory.GetLastCreatedComponent<PlayerPrivateElementComponent<CollisionSphereElementComponent>>();
        _collisionSphereElementComponent.ElementComponent.EntityEntered = EntityEntered;
    }

    public override void Update()
    {
        _collisionSphereElementComponent.ElementComponent.CheckCollisionWith(_playerEntity);
    }

    private void EntityEntered(Entity colshapeEntity, Entity entity)
    {
        ThrowIfDisposed();

        if (entity == Entity)
            Complete(this);
    }

    public override void Dispose()
    {
        if (_collisionSphereElementComponent != null)
        {
            _playerEntity.TryDestroyComponent(_collisionSphereElementComponent);
        }
        _playerEntity.TryDestroyComponent(_markerElementComponent);
        base.Dispose();
    }
}
