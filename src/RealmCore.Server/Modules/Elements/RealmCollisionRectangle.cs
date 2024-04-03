namespace RealmCore.Server.Modules.Elements;

public class RealmCollisionRectangle : CollisionRectangle
{
    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();

    public RealmCollisionRectangle(Vector2 position, Vector2 dimensions) : base(position, dimensions)
    {
    }
}
