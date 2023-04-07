namespace RealmCore.Server.Logic.Registries;

internal class VehicleEnginesRegistryLogic
{
    public VehicleEnginesRegistryLogic(VehicleEnginesRegistry vehicleEnginesRegistry)
    {
        vehicleEnginesRegistry.Add(1, new VehicleEngineRegistryEntry
        {
            UpgradeId = 1,
        });
    }
}
