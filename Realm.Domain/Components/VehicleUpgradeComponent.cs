using SlipeServer.Server.Constants;
using VehicleUpgrade = Realm.Domain.Upgrades.VehicleUpgrade;

namespace Realm.Domain.Components;

[Serializable]
public class VehicleUpgradeComponent : IElementComponent
{
    private RPGVehicle _rpgVehicle = default!;

    [ScriptMember("name")]
    public string Name => "VehicleUpgrades";

    private List<VehicleUpgrade> Upgrades = new();

    public VehicleUpgradeComponent()
    {
    }

    public VehicleUpgradeComponent(SerializationInfo info, StreamingContext context)
    {
        Upgrades = (List<VehicleUpgrade>?)info.GetValue("Upgrades", typeof(List<VehicleUpgrade>)) ?? throw new SerializationException();
    }

    [NoScriptAccess]
    public void SetOwner(Element element)
    {
        if (_rpgVehicle != null)
            throw new Exception("Component already attached to element");
        if (element is not RPGVehicle rpgVehicle)
            throw new Exception("Not supported element type, expected: RPGVehicle");
        _rpgVehicle = rpgVehicle;
        _rpgVehicle.Disposed += Disposed;
        RebuildUpgrades();
    }

    private void Disposed(IPersistantVehicle obj)
    {
        _rpgVehicle.Disposed -= Disposed;
    }

    [ScriptMember("getUpgradesCount")]
    public int GetUpgradesCount()
    {
        return Upgrades.Count;
    }

    [ScriptMember("addUpgrade")]
    public bool AddUpgrade(VehicleUpgrade upgrade, bool rebuild = true)
    {
        Upgrades.Add(upgrade);
        if (rebuild)
            RebuildUpgrades();
        return true;
    }

    private void RebuildUpgrades()
    {
        var handling = VehicleHandlingConstants.DefaultVehicleHandling[_rpgVehicle.Model];
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

        _rpgVehicle.Handling = handling;
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Upgrades", Upgrades);
    }
}
