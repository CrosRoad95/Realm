namespace RealmCore.Server.Modules.Elements;

public class RealmCollisionTube : CollisionTube, ICollisionDetection
{
    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();
    public CollisionDetection<RealmCollisionTube> CollisionDetection { get; private set; }
    public CollisionDetection InternalCollisionDetection => CollisionDetection;

    public RealmCollisionTube(IServiceProvider serviceProvider, Vector3 position, float Radius, float Height) : base(position, Radius, Height)
    {
        CollisionDetection = new(serviceProvider, this);
    }
}
