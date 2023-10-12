using RealmCore.Server.Concepts.Upgrades;

namespace RealmCore.Sample.Logic;

public class VehicleUpgradesLogic
{
    public VehicleUpgradesLogic(VehicleUpgradeRegistry vehicleUpgradeRegistry)
    {
        vehicleUpgradeRegistry.AddUpgrade(2, new VehicleUpgradeRegistryEntry
        {
            EngineAcceleration = new FloatValueUpgradeDescription
            {
                IncreaseByUnits = 100,
                MultipleBy = 2,
            },
            MaxVelocity = new FloatValueUpgradeDescription
            {
                IncreaseByUnits = 100,
                MultipleBy = 2,
            }
        });

        vehicleUpgradeRegistry.AddUpgrade(3, new VehicleUpgradeRegistryEntry
        {
            Visuals = new VisualUpgradeDescription
            {
                Wheels = SlipeServer.Packets.Enums.VehicleUpgrades.VehicleUpgradeWheel.Offroad
            }
        });
    }
}
