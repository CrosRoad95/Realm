namespace RealmCore.Server.Logic.Registries;

internal class VehicleUpgradeRegistryLogic
{
    public VehicleUpgradeRegistryLogic(VehicleUpgradeRegistry vehicleUpgradeRegistry)
    {
        vehicleUpgradeRegistry.AddUpgrade(1, new VehicleUpgradeRegistryEntry());
    }
}
