namespace RealmCore.Persistance.Data;

public sealed class VehicleData
{
#pragma warning disable CS8618
    public int Id { get; set; }
    public ushort Model { get; set; }
    public TransformAndMotion TransformAndMotion { get; set; }
    public VehicleColor Color { get; set; }
    public short Paintjob { get; set; }
    public string Platetext { get; set; }
    public VehicleVariant Variant { get; set; }
    public VehicleDamageState DamageState { get; set; }
    public VehicleDoorOpenRatio DoorOpenRatio { get; set; }
    public VehicleWheelStatus WheelStatus { get; set; }
    public bool EngineState { get; set; }
    public bool LandingGearDown { get; set; }
    public byte OverrideLights { get; set; }
    public bool SirensState { get; set; }
    public bool Locked { get; set; }
    public bool TaxiLightState { get; set; }
    public float Health { get; set; }
    public bool IsFrozen { get; set; }
    public bool Removed { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Spawned { get; set; }
    public float Mileage { get; set; }

    public ICollection<InventoryData> Inventories { get; set; } = new List<InventoryData>();
    public ICollection<VehicleUserAccessData> UserAccesses { get; set; } = new List<VehicleUserAccessData>();
    public ICollection<VehicleUpgradeData> Upgrades { get; set; } = new List<VehicleUpgradeData>();
    public ICollection<VehicleFuelData> Fuels { get; set; } = new List<VehicleFuelData>();
    public ICollection<VehiclePartDamageData> PartDamages { get; set; } = new List<VehiclePartDamageData>();
    public ICollection<VehicleEngineData> VehicleEngines { get; set; } = new List<VehicleEngineData>();
    public ICollection<VehicleInventoryData> VehicleInventories = new List<VehicleInventoryData>();
    public ICollection<VehicleEventData> VehicleEvents = new List<VehicleEventData>();
#pragma warning restore CS8618
}
