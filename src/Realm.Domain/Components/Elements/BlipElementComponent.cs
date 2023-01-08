namespace Realm.Domain.Components.Elements;

public class BlipElementComponent : ElementComponent
{
    protected readonly Blip _blip;

    internal override Element Element => _blip;

    internal BlipElementComponent(Blip blip, Entity? createForEntity = null) : base(createForEntity)
    {
        _blip = blip;
    }
}
