namespace RealmCore.Server.Modules.Elements;

public class RealmMarker : Marker, IElementName
{
    public event Action<Element>? ElementEntered;
    public event Action<Element>? ElementLeft;
    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();
    public CollisionSphere CollisionShape { get; init; }
    public string? ElementName { get; set; }

    public RealmMarker(Vector3 position, MarkerType markerType, float size) : base(position, markerType)
    {
        Size = size;
        CollisionShape = new CollisionSphere(position, size);
    }

    public override bool Destroy()
    {
        CollisionShape.Destroy();
        return base.Destroy();
    }
}
