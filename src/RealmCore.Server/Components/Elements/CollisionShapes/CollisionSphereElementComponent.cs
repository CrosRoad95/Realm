namespace RealmCore.Server.Components.Elements.CollisionShapes;

public class CollisionSphereElementComponent : CollisionSphere, ICollisionShape
{
    public CollisionSphereElementComponent(Vector3 position, float Radius) : base(position, Radius)
    {
    }

    public Entity Entity { get; set; }
}
