namespace RealmCore.Server.Modules.Elements;

public class RealmCollisionSphere : CollisionSphere
{
    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();

    public RealmCollisionSphere(Vector3 position, float Radius) : base(position, Radius)
    {
    }
}
