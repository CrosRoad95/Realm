namespace RealmCore.Server.Registries;

public class VehicleUpgradeRegistry
{
    private readonly Dictionary<int, VehicleUpgradeRegistryEntry> _vehicleUpgradesRegistryEntries = new();
    public VehicleUpgradeRegistry()
    {

    }

    public VehicleUpgradeRegistryEntry Get(int id)
    {
        return _vehicleUpgradesRegistryEntries[id];
    }

    public void AddUpgrade(int id, VehicleUpgradeRegistryEntry vehicleUpgradeRegistryEntry)
    {
        if (_vehicleUpgradesRegistryEntries.ContainsKey(id))
        {
            throw new Exception("Item of id '" + id + "' already exists;.");
        }
        vehicleUpgradeRegistryEntry.Id = id;
        _vehicleUpgradesRegistryEntries[id] = vehicleUpgradeRegistryEntry;
    }
}
