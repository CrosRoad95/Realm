namespace Realm.Domain.Components.Elements;

public class VehicleElementComponent : ElementComponent
{
    protected readonly Vehicle _vehicle;

    public Vehicle Vehicle => _vehicle;

    public override Element Element => _vehicle;

    public VehicleElementComponent(Vehicle vehicle)
    {
        _vehicle = vehicle;
    }

    private void HandleDestroyed(Entity entity)
    {
        Entity.Destroyed -= HandleDestroyed;
        _vehicle.Destroy();
    }

    public override Task Load()
    {
        Entity.GetRequiredService<IRPGServer>().AssociateElement(new ElementHandle(_vehicle));
        Entity.Destroyed += HandleDestroyed;
        Entity.Transform.Bind(_vehicle);
        return Task.CompletedTask;
    }
}
