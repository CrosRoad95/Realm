namespace RealmCore.Server.Modules.Elements;

public class RealmCollisionCuboid : CollisionCuboid
{
    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();

    public RealmCollisionCuboid(Vector3 position, Vector3 dimensions) : base(position, dimensions)
    {
    }
}
