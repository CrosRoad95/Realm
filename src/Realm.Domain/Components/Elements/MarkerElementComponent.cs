namespace Realm.Domain.Components.Elements;

public class MarkerElementComponent : ElementComponent
{

    protected readonly Marker _marker;
    internal override Element Element => _marker;

    public Color Color { get => _marker.Color; set => _marker.Color = value; }

    internal MarkerElementComponent(Marker marker)
    {
        _marker = marker;
    }
}
