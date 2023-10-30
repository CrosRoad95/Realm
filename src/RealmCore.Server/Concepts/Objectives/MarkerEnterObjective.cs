using Microsoft.Extensions.DependencyInjection;

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

    protected override void Load(IServiceProvider serviceProvider, Entity playerEntity)
    {
        _playerEntity = playerEntity;
        var entityFactory = serviceProvider.GetRequiredService<IEntityFactory>();
        using var scopedEntityFactory = entityFactory.CreateScopedEntityFactory(playerEntity);
        scopedEntityFactory.CreateMarker(MarkerType.Arrow, _position, Color.White);
        _markerElementComponent = scopedEntityFactory.GetLastCreatedComponent<PlayerPrivateElementComponent<MarkerElementComponent>>();
        scopedEntityFactory.CreateCollisionSphere(_position, 2);
        _collisionSphereElementComponent = scopedEntityFactory.GetLastCreatedComponent<PlayerPrivateElementComponent<CollisionSphereElementComponent>>();
        _collisionSphereElementComponent.ElementComponent.ElementEntered += HandleElementEntered;
    }

    private void HandleElementEntered(Element element)
    {
        if (Entity == element.TryUpCast())
            Complete(this);
    }

    public override void Update()
    {
        _collisionSphereElementComponent.ElementComponent.CheckElementWithin(_playerEntity.GetElement());
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
