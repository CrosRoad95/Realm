namespace RealmCore.Server.Elements;

public class RealmCollisionSphere : CollisionSphere, ICollisionDetection
{
    public CollisionDetection<RealmCollisionSphere> CollisionDetection { get; private set; }
    public CollisionDetection InternalCollisionDetection => CollisionDetection;

    public RealmCollisionSphere(IServiceProvider serviceProvider, Vector3 position, float Radius) : base(position, Radius)
    {
        CollisionDetection = new(serviceProvider, this);
    }
}
