namespace RealmCore.Server.Components.Elements.CollisionShapes;

public class CollisionTubeElementComponent : CollisionTube, ICollisionShape
{
    public CollisionTubeElementComponent(Vector3 position, float Radius, float Height) : base(position, Radius, Height)
    {
    }

    public Entity Entity { get; set; }
}
