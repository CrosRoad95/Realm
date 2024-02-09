namespace RealmCore.Server.Modules.Elements;

public class RealmCollisionPolygon : CollisionPolygon, ICollisionDetection
{
    public CollisionDetection<RealmCollisionPolygon> CollisionDetection { get; private set; }
    public CollisionDetection InternalCollisionDetection => CollisionDetection;

    public RealmCollisionPolygon(IServiceProvider serviceProvider, Vector3 position, IEnumerable<Vector2> vertices) : base(position, vertices)
    {
        CollisionDetection = new(serviceProvider, this);
    }
}
