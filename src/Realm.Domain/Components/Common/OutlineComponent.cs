namespace Realm.Domain.Components.Common;

public class OutlineComponent : Component
{
    public Color Color { get; }

    public OutlineComponent(Color color)
    {
        Color = color;
    }
}
