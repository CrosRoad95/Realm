namespace RealmCore.Server.Modules.Elements;

public class RealmMarker : Marker, ICollisionDetection, IElementName
{
    public event Action<Element>? ElementEntered;
    public event Action<Element>? ElementLeft;

    public CollisionSphere CollisionShape { get; private set; }
    public CollisionDetection<RealmMarker> CollisionDetection { get; private set; }
    public CollisionDetection InternalCollisionDetection => CollisionDetection;
    public string? ElementName { get; set; }

    public RealmMarker(IServiceProvider serviceProvider, Vector3 position, MarkerType markerType, float size) : base(position, markerType)
    {
        Size = size;
        CollisionDetection = new(serviceProvider, this);
        CollisionShape = new CollisionSphere(position, size);
        CollisionShape.ElementEntered += HandleElementEntered;
        CollisionShape.ElementLeft += HandleElementLeft;
    }

    private void HandleElementLeft(Element leftElement)
    {
        CollisionDetection.RelayLeft(leftElement);
    }

    private void HandleElementEntered(Element enteredElement)
    {
        CollisionDetection.RelayEntered(enteredElement);
    }

    public void CheckElementWithin(Element element)
    {
        CollisionShape.CheckElementWithin(element);
    }
}
