namespace RealmCore.Server.Modules.Elements;

public class RealmCollisionTube : CollisionTube
{
    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();

    public RealmCollisionTube(Vector3 position, float radius, float height) : base(position, radius, height)
    {
    }
}
