using SlipeServer.Packets.Enums.VehicleUpgrades;

namespace Realm.Server.Concepts.Upgrades;

public class VisualUpgradeDescription
{
    public VehicleUpgradeHood? Hood { get; set; }
    public VehicleUpgradeVent? Vent { get; set; }
    public VehicleUpgradeSpoiler? Spoiler { get; set; }
    public VehicleUpgradeSideskirt? Sideskirt { get; set; }
    public VehicleUpgradeFrontBullbar? FrontBullbar { get; set; }
    public VehicleUpgradeRearBullbar? RearBullbar { get; set; }
    public VehicleUpgradeLamp? Lamps { get; set; }
    public VehicleUpgradeRoof? Roof { get; set; }
    public VehicleUpgradeNitro? Nitro { get; set; }
    public bool? HasHydraulics { get; set; }
    public bool? HasStereo { get; set; }
    public VehicleUpgradeWheel? Wheels { get; set; }
    public VehicleUpgradeExhaust? Exhaust { get; set; }
    public VehicleUpgradeFrontBumper? FrontBumper { get; set; }
    public VehicleUpgradeRearBumper? RearBumper { get; set; }
    public VehicleUpgradeMisc? Misc { get; set; }

}
