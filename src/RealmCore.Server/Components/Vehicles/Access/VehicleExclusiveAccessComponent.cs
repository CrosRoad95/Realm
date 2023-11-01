
namespace RealmCore.Server.Components.Vehicles.Access;

public class VehicleExclusiveAccessComponent : VehicleAccessControllerComponent
{
    private readonly Ped _targetEntity;

    public VehicleExclusiveAccessComponent(Ped ped)
    {
        _targetEntity = ped;
        _targetEntity.Destroyed += HandleTargetEntityDestroyed;
    }

    private void HandleTargetEntityDestroyed(Element obj)
    {
        if(Element is RealmVehicle realmVehicle)
            realmVehicle.Components.DestroyComponent(this);
        _targetEntity.Destroyed -= HandleTargetEntityDestroyed;
    }

    protected override bool CanEnter(Ped ped, RealmVehicle vehicle, byte seat) => ped == _targetEntity;
}
