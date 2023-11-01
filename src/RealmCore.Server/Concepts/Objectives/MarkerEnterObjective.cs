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
        //var elementFactory = serviceProvider.GetRequiredService<IElementFactory>();
        //using var scopedelementFactory = elementFactory.CreateScopedelementFactory(playerEntity);
        //scopedelementFactory.CreateMarker(MarkerType.Arrow, _position, Color.White);
        //_markerElementComponent = scopedelementFactory.GetLastCreatedComponent<PlayerPrivateElementComponent<MarkerElementComponent>>();
        //scopedelementFactory.CreateCollisionSphere(_position, 2);
        //_collisionSphereElementComponent = scopedelementFactory.GetLastCreatedComponent<PlayerPrivateElementComponent<CollisionSphereElementComponent>>();
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
