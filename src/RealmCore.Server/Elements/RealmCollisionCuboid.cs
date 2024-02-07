namespace RealmCore.Server.Elements;

public class RealmCollisionCuboid : CollisionCuboid, ICollisionDetection
{
    public Concepts.CollisionDetection<RealmCollisionCuboid> CollisionDetection { get; private set; }
    public CollisionDetection InternalCollisionDetection => CollisionDetection;

    public RealmCollisionCuboid(IServiceProvider serviceProvider, Vector3 position, Vector3 dimensions) : base(position, dimensions)
    {
        CollisionDetection = new(serviceProvider, this);
    }
}
