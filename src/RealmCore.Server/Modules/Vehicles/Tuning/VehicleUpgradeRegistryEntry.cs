namespace RealmCore.Server.Modules.Vehicles.Tuning;

public class VehicleUpgradeRegistryEntry
{
    internal int Id { get; set; }

    public IVehicleHandlingModifier VehicleUpgrade { get; }

    public VehicleUpgradeRegistryEntry(IVehicleHandlingModifier vehicleUpgrade)
    {
        VehicleUpgrade = vehicleUpgrade;
    }
}
