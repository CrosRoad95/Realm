namespace RealmCore.Server.Components.Common;

public class OutlineComponent : Component
{
    private readonly Color _color;

    public Color Color => _color;

    public OutlineComponent(Color color)
    {
        _color = color;
    }
}
