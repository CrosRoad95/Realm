namespace RealmCore.Server.Concepts.Objectives;

public class MarkerEnterObjective : Objective
{
    private readonly Vector3 _position;

    private RealmMarker _marker = default!;

    public override Vector3 Position => _position;

    public MarkerEnterObjective(Vector3 position)
    {
        _position = position;
    }

    protected override void Load(RealmPlayer player)
    {
        // TODO:
        //_playerEntity = playerEntity;
        //var entityFactory = serviceProvider.GetRequiredService<IElementFactory>();
        //using var scopedEntityFactory = entityFactory.CreateScopedEntityFactory(playerEntity);
        //scopedEntityFactory.CreateMarker(MarkerType.Arrow, _position, Color.White);
        //_markerElementComponent = scopedEntityFactory.GetLastCreatedComponent<PlayerPrivateElementComponent<MarkerElementComponent>>();
        //scopedEntityFactory.CreateCollisionSphere(_position, 2);
        //_collisionSphereElementComponent = scopedEntityFactory.GetLastCreatedComponent<PlayerPrivateElementComponent<CollisionSphereElementComponent>>();
        //_collisionSphereElementComponent.ElementComponent.ElementEntered += HandleElementEntered;
    }

    private void HandleElementEntered(Element element)
    {
        if (Player == element)
            Complete(this);
    }

    public override void Update()
    {
        _marker.CollisionShape.CheckElementWithin(Player);
    }

    public override void Dispose()
    {
        if (_marker != null)
            _marker.Destroy();
        base.Dispose();
    }
}
