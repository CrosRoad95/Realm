namespace RealmCore.Server.Elements;

public class RealmCollisionCircle : CollisionCircle, ICollisionDetection
{
    public Concepts.CollisionDetection<RealmCollisionCircle> CollisionDetection { get; private set; }
    public CollisionDetection InternalCollisionDetection => CollisionDetection;

    public RealmCollisionCircle(IServiceProvider serviceProvider, Vector2 position, float radius) : base(position, radius)
    {
        CollisionDetection = new(serviceProvider, this);
    }
}
