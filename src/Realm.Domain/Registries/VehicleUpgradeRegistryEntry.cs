using Realm.Domain.Data.VehicleUpgrades;

namespace Realm.Domain.Registries;

public class VehicleUpgradeRegistryEntry
{
    internal int Id { get; set; }

    public FloatValueUpgradeDescription? MaxVelocity { get; set; } = null;
    public FloatValueUpgradeDescription? EngineAcceleration { get; set; } = null;
    public VisualUpgradeDescription? Visuals { get; set; } = null;
}
