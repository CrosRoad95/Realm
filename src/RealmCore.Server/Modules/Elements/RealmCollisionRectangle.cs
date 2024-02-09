namespace RealmCore.Server.Modules.Elements;

public class RealmCollisionRectangle : CollisionRectangle, ICollisionDetection
{
    public CollisionDetection<RealmCollisionRectangle> CollisionDetection { get; private set; }
    public CollisionDetection InternalCollisionDetection => CollisionDetection;

    public RealmCollisionRectangle(IServiceProvider serviceProvider, Vector2 position, Vector2 dimensions) : base(position, dimensions)
    {
        CollisionDetection = new(serviceProvider, this);
    }
}
