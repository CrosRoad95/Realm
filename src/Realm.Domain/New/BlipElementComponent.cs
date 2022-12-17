using Realm.Interfaces.Server;

namespace Realm.Domain.New;

public class BlipElementComponent : Component
{
    private Blip _blip;

    public BlipElementComponent(BlipIcon icon)
    {
        _blip = new Blip(Entity.Transform.Position, icon);
    }

    private void HandleDestroyed(Entity entity)
    {
        _blip.Destroy();
    }

    public override Task Load()
    {
        Entity.GetRequiredService<IRPGServer>().AssociateElement(new ElementHandle(_blip));
        Entity.Destroyed += HandleDestroyed;
        return Task.CompletedTask;
    }
}
