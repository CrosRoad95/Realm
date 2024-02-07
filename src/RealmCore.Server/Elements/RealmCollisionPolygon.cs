namespace RealmCore.Server.Elements;

public class RealmCollisionPolygon : CollisionPolygon, ICollisionDetection
{
    public Concepts.CollisionDetection<RealmCollisionPolygon> CollisionDetection { get; private set; }
    public CollisionDetection InternalCollisionDetection => CollisionDetection;

    public RealmCollisionPolygon(IServiceProvider serviceProvider, Vector3 position, IEnumerable<Vector2> vertices) : base(position, vertices)
    {
        CollisionDetection = new(serviceProvider, this);
    }
}
