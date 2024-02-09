namespace RealmCore.Server.Modules.Vehicles.Access.Controllers;

public class VehicleLicenseRequirementAccessController : VehicleAccessController
{
    private readonly int _licenseId;

    public VehicleLicenseRequirementAccessController(int licenseId)
    {
        _licenseId = licenseId;
    }

    protected override bool CanEnter(Ped ped, RealmVehicle vehicle, byte seat)
    {
        if (ped is RealmPlayer player)
            return player.Licenses.Has(_licenseId);

        return false;
    }
}
