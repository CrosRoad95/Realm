namespace RealmCore.Server.Modules.Elements;

public class RealmCollisionPolygon : CollisionPolygon, ICollisionDetection
{
    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();
    public CollisionDetection<RealmCollisionPolygon> CollisionDetection { get; private set; }
    public CollisionDetection InternalCollisionDetection => CollisionDetection;

    public RealmCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices) : base(position, vertices)
    {
        CollisionDetection = new(this);
    }
}
