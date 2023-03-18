namespace Realm.Console.Logic;

public class VehicleUpgradesLogic
{
    public VehicleUpgradesLogic(VehicleUpgradeRegistry vehicleUpgradeRegistry)
    {
        vehicleUpgradeRegistry.AddUpgrade(2, new VehicleUpgradeRegistryEntry
        {
            EngineAcceleration = new Domain.Data.FloatValueUpgradeDescription
            {
                IncreaseByUnits = 100,
                MultipleBy = 2,
            },
            MaxVelocity = new Domain.Data.FloatValueUpgradeDescription
            {
                IncreaseByUnits = 100,
                MultipleBy = 2,
            }
        });
    }
}
