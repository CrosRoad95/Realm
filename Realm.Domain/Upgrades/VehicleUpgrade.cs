﻿namespace Realm.Domain.Upgrades;

// TODO: rename
public class VehicleUpgrade
{
    public class UpgradeDescription
    {
        [NoScriptAccess]
        public float IncreaseByUnits { get; set; }
        [NoScriptAccess]
        public float MultipleBy { get; set; }

        [JsonConstructor]
        public UpgradeDescription(float increaseByUnits, float multipleBy = 1)
        {
            IncreaseByUnits = increaseByUnits;
            MultipleBy = multipleBy;
        }
        public UpgradeDescription(float[] values)
        {
            IncreaseByUnits = values[0];
            MultipleBy = values[1];
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