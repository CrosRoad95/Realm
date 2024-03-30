namespace RealmCore.Server.Modules.Elements;

public class RealmCollisionRectangle : CollisionRectangle, ICollisionDetection
{
    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();
    public CollisionDetection<RealmCollisionRectangle> CollisionDetection { get; private set; }
    public CollisionDetection InternalCollisionDetection => CollisionDetection;

    public RealmCollisionRectangle(Vector2 position, Vector2 dimensions) : base(position, dimensions)
    {
        CollisionDetection = new(this);
    }
}
