namespace RealmCore.Server.Components.Vehicles.Access;

[ComponentUsage(true)]
public abstract class VehicleAccessControllerComponent : Component
{
    protected abstract bool CanEnter(Ped ped, Vehicle vehicle);

    protected override void Load()
    {
        if (Entity.Tag != EntityTag.Vehicle)
            throw new NotSupportedException("This component only works on vehicles.");

        if (Entity.Components.OfType<VehicleAccessControllerComponent>().Where(x => x != this).Any())
            throw new InvalidOperationException("Vehicle already have vehicle access controller component");

        Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle.CanEnter = CanEnter;
    }

    protected override void Detached()
    {
        Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle.CanEnter = null;
    }
}
