namespace RealmCore.Server.Modules.Elements;

public class RealmCollisionCuboid : CollisionCuboid, ICollisionDetection
{
    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();
    public CollisionDetection<RealmCollisionCuboid> CollisionDetection { get; private set; }
    public CollisionDetection InternalCollisionDetection => CollisionDetection;

    public RealmCollisionCuboid(IServiceProvider serviceProvider, Vector3 position, Vector3 dimensions) : base(position, dimensions)
    {
        CollisionDetection = new(serviceProvider, this);
    }
}
