namespace Realm.Domain.Components.Elements;

public class MarkerElementComponent : ElementComponent
{

    protected readonly Marker _marker;
    internal override Element Element => _marker;

    public MarkerElementComponent(Marker marker, Entity? createForEntity = null) : base(createForEntity)
    {
        _marker = marker;
    }
}
