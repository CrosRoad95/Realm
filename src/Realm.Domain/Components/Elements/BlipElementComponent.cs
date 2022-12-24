namespace Realm.Domain.Components.Elements;

public class BlipElementComponent : Component
{
    private readonly Blip _blip;

    public BlipElementComponent(BlipIcon icon)
    {
        _blip = new Blip(Vector3.Zero, icon);
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
