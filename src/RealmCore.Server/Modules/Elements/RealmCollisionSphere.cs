namespace RealmCore.Server.Modules.Elements;

public class RealmCollisionSphere : CollisionSphere, ICollisionDetection
{
    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();
    public CollisionDetection<RealmCollisionSphere> CollisionDetection { get; private set; }
    public CollisionDetection InternalCollisionDetection => CollisionDetection;

    public RealmCollisionSphere(IServiceProvider serviceProvider, Vector3 position, float Radius) : base(position, Radius)
    {
        CollisionDetection = new(serviceProvider, this);
        ElementEntered += HandleElementEntered;
        ElementLeft += HandleElementLeft;
    }

    private void HandleElementEntered(Element enteredElement)
    {
        CollisionDetection.RelayEntered(enteredElement);
    }

    private void HandleElementLeft(Element leftElement)
    {
        CollisionDetection.RelayLeft(leftElement);
    }
}
