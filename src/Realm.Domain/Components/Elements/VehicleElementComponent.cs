namespace Realm.Domain.Components.Elements;

public class VehicleElementComponent : ElementComponent
{
    [Inject]
    private IRPGServer rpgServer { get; set; } = default!;

    protected readonly Vehicle _vehicle;

    internal Vehicle Vehicle => _vehicle;

    internal override Element Element => _vehicle;

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
        rpgServer.AssociateElement(new ElementHandle(_vehicle));
        Entity.Destroyed += HandleDestroyed;
        Entity.Transform.Bind(_vehicle);
        return Task.CompletedTask;
    }
}
