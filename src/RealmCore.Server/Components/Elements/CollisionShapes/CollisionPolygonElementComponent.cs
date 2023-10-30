
namespace RealmCore.Server.Components.Elements.CollisionShapes;

public class CollisionPolygonElementComponent : CollisionPolygon, ICollisionShape
{
    public CollisionPolygonElementComponent(Vector3 position, IEnumerable<Vector2> vertices) : base(position, vertices)
    {
    }

    public Entity Entity { get; set; }
}
