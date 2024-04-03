namespace RealmCore.Server.Modules.Elements;

public class RealmCollisionPolygon : CollisionPolygon
{
    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();

    public RealmCollisionPolygon(Vector3 position, IEnumerable<Vector2> vertices) : base(position, vertices)
    {
    }
}
