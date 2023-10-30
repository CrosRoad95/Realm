
namespace RealmCore.Server.Components.Elements.CollisionShapes;

public class CollisionCuboidElementComponent : CollisionCuboid, ICollisionShape
{
    public CollisionCuboidElementComponent(Vector3 position, Vector3 dimensions) : base(position, dimensions)
    {
    }

    public Entity Entity { get; set; }
}
