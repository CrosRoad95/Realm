namespace RealmCore.Server.Components.Vehicles.Access;

public class VehicleLicenseRequiremenetAccessComponent : VehicleAccessControllerComponent
{
    private readonly int _licenseId;

    public VehicleLicenseRequiremenetAccessComponent(int licenseId)
    {
        _licenseId = licenseId;
    }

    protected override bool CanEnter(Entity pedEntity, Entity vehicleEntity)
    {
        if(pedEntity.TryGetComponent(out LicensesComponent licensesComponent))
        {
            return licensesComponent.HasLicense(_licenseId);
        }
        return false;
    }
}
