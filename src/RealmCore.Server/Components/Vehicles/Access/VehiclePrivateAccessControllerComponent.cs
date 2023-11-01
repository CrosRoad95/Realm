namespace RealmCore.Server.Components.Vehicles.Access;

public class VehiclePrivateAccessControllerComponent : VehicleAccessControllerComponent
{
    private PrivateVehicleComponent? _privateVehicleComponent;

    protected override bool CanEnter(Ped ped, RealmVehicle vehicle, byte seat)
    {
        if (ped is not RealmPlayer player)
            return false;

        if (_privateVehicleComponent == null)
            _privateVehicleComponent = vehicle.Components.GetRequiredComponent<PrivateVehicleComponent>();

        return _privateVehicleComponent.Access.HasAccess(player);
    }
}
