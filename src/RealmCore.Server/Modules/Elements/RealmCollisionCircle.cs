namespace RealmCore.Server.Modules.Elements;

public class RealmCollisionCircle : CollisionCircle, ICollisionDetection
{
    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();
    public CollisionDetection<RealmCollisionCircle> CollisionDetection { get; private set; }
    public CollisionDetection InternalCollisionDetection => CollisionDetection;

    public RealmCollisionCircle(Vector2 position, float radius) : base(position, radius)
    {
        CollisionDetection = new(this);
    }
}
