namespace RealmCore.Server.Components.Vehicles.Access;

public class VehicleExclusiveAccessComponent : VehicleAccessControllerComponent
{
    private readonly Ped _ped;

    public VehicleExclusiveAccessComponent(Ped ped)
    {
        _ped = ped;
        _ped.Destroyed += HandleTargetElementDestroyed;
    }

    private void HandleTargetElementDestroyed(Element obj)
    {
        if(Element is RealmVehicle vehicle)
            vehicle.DestroyComponent(this);
        _ped.Destroyed -= HandleTargetElementDestroyed;
    }

    protected override bool CanEnter(Ped ped, RealmVehicle vehicle, byte seat) => ped == _ped;
}
