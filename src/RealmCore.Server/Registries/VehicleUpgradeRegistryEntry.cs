﻿namespace RealmCore.Server.Registries;

public class VehicleUpgradeRegistryEntry
{
    internal int Id { get; set; }

    public FloatValueUpgradeDescription? MaxVelocity { get; set; } = null;
    public FloatValueUpgradeDescription? EngineAcceleration { get; set; } = null;
    public VisualUpgradeDescription? Visuals { get; set; } = null;
}