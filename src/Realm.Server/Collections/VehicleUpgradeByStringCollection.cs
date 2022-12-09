using VehicleUpgrade = Realm.Domain.Upgrades.VehicleUpgrade;

namespace Realm.Server.Collections;

public sealed class VehicleUpgradeByStringCollection
{
    private readonly Dictionary<string, VehicleUpgrade> _vehicleUpgradeById = new();

    public bool AssignElementToId(VehicleUpgrade vehicleUpgrade, string id)
    {
        if (_vehicleUpgradeById.ContainsKey(id))
            return false;
        _vehicleUpgradeById[id] = vehicleUpgrade;
        return true;
    }

    public VehicleUpgrade? GetElementById(string id)
    {
        if (_vehicleUpgradeById.TryGetValue(id, out VehicleUpgrade? vehicleUpgrade))
            return vehicleUpgrade;
        return null;
    }
}
