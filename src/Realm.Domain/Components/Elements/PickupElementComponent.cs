namespace Realm.Domain.Components.Elements;

public class PickupElementComponent : Component
{
    private Pickup _pickup;

    public PickupElementComponent(ushort model)
    {
        _pickup = new Pickup(Vector3.Zero, model);
    }

    private void HandleDestroyed(Entity entity)
    {
        _pickup.Destroy();
    }

    public override Task Load()
    {
        _pickup.Position = Entity.Transform.Position;
        Entity.GetRequiredService<IRPGServer>().AssociateElement(new ElementHandle(_pickup));
        Entity.Destroyed += HandleDestroyed;
        return Task.CompletedTask;
    }
}
