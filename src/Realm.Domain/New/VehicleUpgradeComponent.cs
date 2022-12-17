using SlipeServer.Server.Constants;
using VehicleUpgrade = Realm.Domain.Upgrades.VehicleUpgrade;

namespace Realm.Domain.New;

[Serializable]
public class VehicleUpgradeComponent : Component
{

    private List<VehicleUpgrade> Upgrades = new();

    public VehicleUpgradeComponent()
    {
    }

    [ScriptMember("getUpgradesCount")]
    public int GetUpgradesCount()
    {
        return Upgrades.Count;
    }

    [ScriptMember("addUpgrade")]
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
        var vehicle = Entity.InternalGetRequiredComponent<VehicleElementComponent>().Vehicle;
        var handling = VehicleHandlingConstants.DefaultVehicleHandling[vehicle.Model];
        foreach (var upgrade in Upgrades.Select(x => x.MaxVelocity).Where(x => x != null))
        {
            handling.MaxVelocity += upgrade.IncreaseByUnits;
            handling.MaxVelocity *= upgrade.MultipleBy;
        }
        foreach (var upgrade in Upgrades.Select(x => x.EngineAcceleration).Where(x => x != null))
        {
            handling.EngineAcceleration += upgrade.IncreaseByUnits;
            handling.EngineAcceleration *= upgrade.MultipleBy;
        }

        vehicle.Handling = handling;
    }
}
