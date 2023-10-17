namespace RealmCore.Server.Components.Vehicles.Access;

public class VehiclePrivateAccessControllerComponent : VehicleAccessControllerComponent
{
    private PrivateVehicleComponent? _privateVehicleComponent;

    protected override bool CanEnter(Entity pedEntity, Entity vehicleEntity)
    {
        if (_privateVehicleComponent == null)
            _privateVehicleComponent = Entity.GetRequiredComponent<PrivateVehicleComponent>();

        return _privateVehicleComponent.Access.HasAccess(pedEntity);
    }
}
