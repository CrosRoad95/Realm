using Realm.Interfaces.Server;

namespace Realm.Domain.New;

public class PickupElementComponent : Component
{
    private Pickup _pickup;

    public PickupElementComponent(ushort model)
    {
        _pickup = new Pickup(Entity.Transform.Position, model);
    }

    private void HandleDestroyed(Entity entity)
    {
        _pickup.Destroy();
    }

    public override Task Load()
    {
        Entity.GetRequiredService<IRPGServer>().AssociateElement(new ElementHandle(_pickup));
        Entity.Destroyed += HandleDestroyed;
        return Task.CompletedTask;
    }
}
