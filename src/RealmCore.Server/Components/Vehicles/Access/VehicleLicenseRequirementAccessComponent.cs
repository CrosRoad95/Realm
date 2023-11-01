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
        {
            if(player.Components.TryGetComponent(out LicensesComponent licensesComponent))
            {
                return licensesComponent.HasLicense(_licenseId);
            }
        }
        return false;
    }
}
