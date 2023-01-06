namespace Realm.Console.Logic;

public class VehicleUpgradesLogic
{
    public VehicleUpgradesLogic(VehicleUpgradeRegistry vehicleUpgradeRegistry)
    {
        vehicleUpgradeRegistry.AddUpgrade(1, new VehicleUpgradeRegistryEntry
        {
            EngineAcceleration = new VehicleUpgradeRegistryEntry.UpgradeDescription
            {
                IncreaseByUnits = 100,
                MultipleBy = 2,
            },
            MaxVelocity = new VehicleUpgradeRegistryEntry.UpgradeDescription
            {
                IncreaseByUnits = 100,
                MultipleBy = 2,
            }
        });
    }
}
