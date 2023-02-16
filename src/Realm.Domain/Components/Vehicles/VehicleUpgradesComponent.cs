using VehicleUpgradeData = Realm.Persistance.Data.VehicleUpgrade;

namespace Realm.Domain.Components.Vehicles;

public class VehicleUpgradesComponent : Component
{
    [Inject]
    private VehicleUpgradeRegistry VehicleUpgradeRegistry { get; set; } = default!;

    private readonly List<int> _upgrades = new();
    private readonly object _upgradesLock = new();

    public IReadOnlyCollection<int> Upgrades => _upgrades;

    public VehicleUpgradesComponent()
    {
    }

    internal VehicleUpgradesComponent(ICollection<VehicleUpgradeData> vehicleUpgrades)
    {
        _upgrades = vehicleUpgrades.Select(x => x.UpgradeId).ToList();
    }

    protected override void Load()
    {
        RebuildUpgrades();
    }

    private bool InternalHasUpgrade(int upgradeId) => _upgrades.Any(x => x == upgradeId);

    public bool HasUpgrade(int upgradeId)
    {
        lock (_upgradesLock)
            return InternalHasUpgrade(upgradeId);
    }

    public bool AddUpgrade(int upgradeId, bool rebuild = true)
    {
        lock (_upgradesLock)
            _upgrades.Add(upgradeId);
        if (rebuild)
            RebuildUpgrades();
        return true;
    }

    public bool AddUniqueUpgrade(int upgradeId, bool rebuild = true)
    {
        lock (_upgradesLock)
        {
            if (InternalHasUpgrade(upgradeId))
                return false;
        }
        lock (_upgradesLock)
            _upgrades.Add(upgradeId);
        if (rebuild)
            RebuildUpgrades();
        return true;
    }

    public bool RemoveUpgrade(int upgradeId, bool rebuild = true)
    {
        bool result = _upgrades.Remove(upgradeId);
        if (result && rebuild)
            RebuildUpgrades();
        return result;
    }

    public void RebuildUpgrades()
    {
        lock (_upgradesLock)
        {
            if (!_upgrades.Any())
                return;

            var upgradesEntries = _upgrades.Select(VehicleUpgradeRegistry.Get);
            var vehicle = Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle;
            var handling = VehicleHandlingConstants.DefaultVehicleHandling[vehicle.Model];
            foreach (var upgrade in upgradesEntries.Select(x => x.MaxVelocity).Where(x => x != null).Select(x => x!))
            {
                handling.MaxVelocity += upgrade.IncreaseByUnits;
                handling.MaxVelocity *= upgrade.MultipleBy;
            }
            foreach (var upgrade in upgradesEntries.Select(x => x.EngineAcceleration).Where(x => x != null).Select(x => x!))
            {
                handling.EngineAcceleration += upgrade.IncreaseByUnits;
                handling.EngineAcceleration *= upgrade.MultipleBy;
            }

            vehicle.Handling = handling;
        }
    }
}
