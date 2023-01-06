namespace Realm.Domain.Registries;

public class VehicleUpgradeRegistry
{
    private readonly Dictionary<uint, VehicleUpgradeRegistryEntry> _vehicleUpgradesRegistryEntries = new();
    public VehicleUpgradeRegistry()
    {

    }

    public VehicleUpgradeRegistryEntry Get(uint id)
    {
        return _vehicleUpgradesRegistryEntries[id];
    }

    public void AddUpgrade(uint id, VehicleUpgradeRegistryEntry vehicleUpgradeRegistryEntry)
    {
        if (_vehicleUpgradesRegistryEntries.ContainsKey(id))
        {
            throw new Exception("Item of id '" + id + "' already exists;.");
        }
        vehicleUpgradeRegistryEntry.Id = id;
        _vehicleUpgradesRegistryEntries[id] = vehicleUpgradeRegistryEntry;
    }
}
