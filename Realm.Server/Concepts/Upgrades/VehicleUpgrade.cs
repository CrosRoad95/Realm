namespace Realm.Server.Concepts.Handling;

public class VehicleUpgrade
{
    public class UpgradeDescription
    {
        [NoScriptAccess]
        public float IncreaseByUnits { get; private set; }
        [NoScriptAccess]
        public float MultipleBy { get; private set; }
        public UpgradeDescription(float increaseByUnits, float multipleBy = 1)
        {
            IncreaseByUnits = increaseByUnits;
            MultipleBy = multipleBy;
        }
    }

    [ScriptMember("maxVelocity")]
    public UpgradeDescription? MaxVelocity { get; set; } = null;
    [ScriptMember("engineAcceleration")]
    public UpgradeDescription? EngineAcceleration { get; set; } = null;

    public VehicleUpgrade()
    {
    }


    public override string ToString() => "VehicleUpgrade";
}
