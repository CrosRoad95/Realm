namespace RealmCore.Server.Components.Vehicles.Access;

public class PrivateVehicleAccessControllerComponent : VehicleAccessControllerComponent
{
    [Inject]
    private IECS ECS { get; set; } = default!;

    private PrivateVehicleComponent? _privateVehicleComponent;
    protected override void Load()
    {
        base.Load();
        Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle.CanEnter = CanEnter;
    }

    protected override bool CanEnter(Ped ped, Vehicle vehicle)
    {
        if(_privateVehicleComponent == null)
            _privateVehicleComponent = Entity.GetRequiredComponent<PrivateVehicleComponent>();

        if (ped is not Player player)
            return true;

        if (!ECS.TryGetEntityByPlayer(player, out Entity entity))
            return false;

        return _privateVehicleComponent.Access.HasAccess(entity);
    }

    public override void Dispose()
    {
        Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle.CanEnter = null;
        base.Dispose();
    }
}
