namespace RealmCore.Server.Modules.Elements;

public class RealmRadarArea : RadarArea
{
    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();
    public RealmRadarArea(Vector2 position, Vector2 size, Color color) : base(position, size, color)
    {
    }
}
