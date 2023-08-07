namespace RealmCore.Server.Components.Vehicles.Access;

[ComponentUsage(true)]
public abstract class VehicleAccessControllerComponent : Component
{
    [Inject]
    private IVehicleAccessService VehicleAccessService { get; set; } = default!;

    protected abstract bool CanEnter(Entity pedEntity, Entity vehicleEntity);

    protected override void Load()
    {
        if (Entity.Tag != EntityTag.Vehicle)
            throw new NotSupportedException("This component only works on vehicles.");

        if (Entity.Components.OfType<VehicleAccessControllerComponent>().Where(x => x != this).Any())
            throw new InvalidOperationException("Vehicle already have vehicle access controller component");

        Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle.CanEnter = CanEnterInternal;
    }

    private bool CanEnterInternal(Ped ped, Vehicle vehicle)
    {
        if (VehicleAccessService.InternalCanEnter(ped, vehicle, out var pedEntity, out var vehicleEntity))
            return true;

        return CanEnter(pedEntity, vehicleEntity);
    }

    protected override void Detached()
    {
        Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle.CanEnter = null;
    }
}
