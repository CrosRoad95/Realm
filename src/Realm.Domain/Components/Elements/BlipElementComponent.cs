namespace Realm.Domain.Components.Elements;

public class BlipElementComponent : ElementComponent
{
    protected readonly Blip _blip;

    public override Element Element => _blip;

    public BlipElementComponent(Blip blip, Entity? createForEntity = null) : base(createForEntity)
    {
        _blip = blip;
    }
}
