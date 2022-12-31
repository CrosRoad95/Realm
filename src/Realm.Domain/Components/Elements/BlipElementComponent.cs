namespace Realm.Domain.Components.Elements;

public class BlipElementComponent : ElementComponent
{
    protected readonly Blip _blip;

    public override Element Element => _blip;

    public BlipElementComponent(Blip blip)
    {
        _blip = blip;
    }

    private void HandleDestroyed(Entity entity)
    {
        _blip.Destroy();
    }

    public override Task Load()
    {
        _blip.Position = Entity.Transform.Position;
        Entity.GetRequiredService<IRPGServer>().AssociateElement(new ElementHandle(_blip));
        Entity.Destroyed += HandleDestroyed;
        return Task.CompletedTask;
    }
}
