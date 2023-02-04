namespace Realm.Domain.Registries;

public class VehicleUpgradeRegistryEntry
{
    internal int Id { get; set; }
    public class UpgradeDescription
    {
        public float IncreaseByUnits { get; set; } = 0;
        public float MultipleBy { get; set; } = 1;
    }

    public UpgradeDescription? MaxVelocity { get; set; } = null;
    public UpgradeDescription? EngineAcceleration { get; set; } = null;
}
