namespace RealmCore.Server.Modules.Elements;

public class RealmCollisionTube : CollisionTube, ICollisionDetection
{
    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();
    public CollisionDetection<RealmCollisionTube> CollisionDetection { get; private set; }
    public CollisionDetection InternalCollisionDetection => CollisionDetection;

    public RealmCollisionTube(Vector3 position, float radius, float height) : base(position, radius, height)
    {
        CollisionDetection = new(this);
    }
}
