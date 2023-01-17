namespace Realm.Domain.Components.Elements;

public class MarkerElementComponent : ElementComponent
{

    protected readonly Marker _marker;
    internal override Element Element => _marker;

    internal MarkerElementComponent(Marker marker)
    {
        _marker = marker;
    }
}
