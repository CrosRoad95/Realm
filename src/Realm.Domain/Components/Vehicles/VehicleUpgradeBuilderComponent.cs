using SlipeServer.Server.Constants;
using VehicleUpgrade = Realm.Domain.Upgrades.VehicleUpgrade;

namespace Realm.Domain.Components.Vehicles;

[Serializable]
public class VehicleUpgradeBuilderComponent : Component
{

    private List<VehicleUpgrade> Upgrades = new();

    public VehicleUpgradeBuilderComponent()
    {
    }

    public int GetUpgradesCount()
    {
        return Upgrades.Count;
    }

    public bool AddUpgrade(VehicleUpgrade upgrade, bool rebuild = true)
    {
        if (upgrade == null)
            throw new NullReferenceException(nameof(upgrade));
        Upgrades.Add(upgrade);
        if (rebuild)
            RebuildUpgrades();
        return true;
    }

    private void RebuildUpgrades()
    {
        var vehicle = Entity.GetRequiredComponent<VehicleElementComponent>().Vehicle;
        var handling = VehicleHandlingConstants.DefaultVehicleHandling[vehicle.Model];
        foreach (var upgrade in Upgrades.Select(x => x.MaxVelocity).Where(x => x != null).Select(x => x!))
        {
            handling.MaxVelocity += upgrade.IncreaseByUnits;
            handling.MaxVelocity *= upgrade.MultipleBy;
        }
        foreach (var upgrade in Upgrades.Select(x => x.EngineAcceleration).Where(x => x != null).Select(x => x!))
        {
            handling.EngineAcceleration += upgrade.IncreaseByUnits;
            handling.EngineAcceleration *= upgrade.MultipleBy;
        }

        vehicle.Handling = handling;
    }
}
