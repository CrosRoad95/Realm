using VehicleUpgradeData = Realm.Persistance.Data.VehicleUpgrade;

namespace Realm.Domain.Components.Vehicles;

public class VehicleUpgradesComponent : Component
{
    [Inject]
    private VehicleUpgradeRegistry VehicleUpgradeRegistry { get; set; } = default!;

    private List<uint> _upgrades = new();
    public IEnumerable<uint> Upgrades => _upgrades;

    public VehicleUpgradesComponent()
    {
    }
    
    public VehicleUpgradesComponent(ICollection<VehicleUpgradeData> vehicleUpgrades)
    {
        _upgrades = vehicleUpgrades.Select(x => x.UpgradeId).ToList();
    }

    public override Task Load()
    {
        RebuildUpgrades();
        return Task.CompletedTask;
    }

    public bool HasUpgrade(uint upgradeId) => _upgrades.Any(x => x == upgradeId);

    public bool AddUpgrade(uint upgradeId, bool rebuild = true)
    {
        _upgrades.Add(upgradeId);
        if (rebuild)
            RebuildUpgrades();
        return true;
    }
    
    public bool RemoveUpgrade(uint upgradeId, bool rebuild = true)
    {
        bool result = _upgrades.Remove(upgradeId);
        if (result && rebuild)
            RebuildUpgrades();
        return result;
    }

    public void RebuildUpgrades()
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
