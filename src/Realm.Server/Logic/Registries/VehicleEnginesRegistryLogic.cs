using Realm.Domain.Registries;

namespace Realm.Server.Logic.Registries;

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
