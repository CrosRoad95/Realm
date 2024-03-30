namespace RealmCore.Server.Modules.Pickups;

public class RealmPickup : Pickup, ICollisionDetection, IElementName
{
    public event Action<Element>? ElementEntered;
    public event Action<Element>? ElementLeft;

    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();
    public string? ElementName { get; set; }
    public CollisionDetection<RealmPickup> CollisionDetection { get; private set; }
    public CollisionDetection InternalCollisionDetection => CollisionDetection;

    public RealmPickup(Vector3 position, ushort model) : base(position, model)
    {
        CollisionDetection = new(this);
        CollisionShape = new RealmCollisionSphere(position, 1.5f);
        CollisionShape.ElementEntered += HandleElementEntered;
        CollisionShape.ElementLeft += HandleElementLeft;
    }

    private void HandleElementEntered(Element enteredElement)
    {
        CollisionDetection.RelayEntered(enteredElement);
    }

    private void HandleElementLeft(Element leftElement)
    {
        CollisionDetection.RelayLeft(leftElement);
    }

    public void CheckElementWithin(Element element, bool matchInterior = true, bool matchDimension = true)
    {
        CollisionShape.CheckElementWithin(element, matchInterior, matchDimension);
    }
}
