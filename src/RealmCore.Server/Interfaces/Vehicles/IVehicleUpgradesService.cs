

namespace RealmCore.Server.Interfaces.Vehicles;

public interface IVehicleUpgradesService : IVehicleService, IEnumerable<int>
{
    IReadOnlyCollection<int> Upgrades { get; }

    event Action<IVehicleUpgradesService, int>? UpgradeAdded;
    event Action<IVehicleUpgradesService, int>? UpgradeRemoved;
    internal event Action<IVehicleUpgradesService>? Rebuild;

    bool AddUniqueUpgrade(int upgradeId, bool rebuild = true);
    bool AddUpgrade(int upgradeId, bool rebuild = true);
    bool AddUpgrades(IEnumerable<int> upgradeIds, bool rebuild = true);
    bool HasUpgrade(int upgradeId);
    void RemoveAllUpgrades(bool rebuild = true);
    bool RemoveUpgrade(int upgradeId, bool rebuild = true);
}
