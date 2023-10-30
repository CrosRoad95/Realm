namespace RealmCore.Server.Components.Elements;

public class RadarAreaElementComponent : RadarArea, IElementComponent
{
    public RadarAreaElementComponent(Vector2 position, Vector2 size, Color color) : base(position, size, color)
    {
    }

    public Entity Entity { get; set; }
}
