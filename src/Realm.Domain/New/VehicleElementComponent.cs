using Realm.Interfaces.Server;

namespace Realm.Domain.New;

[NoDefaultScriptAccess]
public sealed class VehicleElementComponent : Component
{
    private readonly Vehicle _vehicle;

    public Vehicle Vehicle => _vehicle;

    public VehicleElementComponent(ushort model)
    {
        _vehicle = new Vehicle(model, Vector3.Zero);
    }

    private void HandleDestroyed(Entity entity)
    {
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
