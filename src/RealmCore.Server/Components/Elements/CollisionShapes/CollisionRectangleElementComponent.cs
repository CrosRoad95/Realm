namespace RealmCore.Server.Components.Elements.CollisionShapes;

public class CollisionRectangleElementComponent : CollisionRectangle, ICollisionShape
{
    public CollisionRectangleElementComponent(Vector2 position, Vector2 dimensions) : base(position, dimensions)
    {
    }

    public Entity Entity { get; set; }
}
