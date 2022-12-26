namespace Realm.Domain.Components.Elements;

public class PickupElementComponent : Component
{
    protected readonly Pickup _pickup;

    public PickupElementComponent(Pickup pickup)
    {
        _pickup = pickup;
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
