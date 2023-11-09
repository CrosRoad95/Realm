namespace RealmCore.Server.Components.Vehicles.Access;

public class VehicleLicenseRequirementAccessComponent : VehicleAccessControllerComponent
{
    private readonly int _licenseId;

    public VehicleLicenseRequirementAccessComponent(int licenseId)
    {
        _licenseId = licenseId;
    }

    protected override bool CanEnter(Ped ped, RealmVehicle vehicle, byte seat)
    {
        if(ped is RealmPlayer player)
            return player.Licenses.Has(_licenseId);

        return false;
    }
}
