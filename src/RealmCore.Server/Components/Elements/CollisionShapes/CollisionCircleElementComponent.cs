

namespace RealmCore.Server.Components.Elements.CollisionShapes;

public class CollisionCircleElementComponent : CollisionCircle, ICollisionShape
{
    public CollisionCircleElementComponent(Vector2 position, float Radius) : base(position, Radius)
    {
    }

    public Entity Entity { get; set; }
}
