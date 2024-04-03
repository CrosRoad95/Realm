namespace RealmCore.Server.Modules.Elements;

public class RealmCollisionCircle : CollisionCircle
{
    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();

    public RealmCollisionCircle(Vector2 position, float radius) : base(position, radius)
    {
    }
}
