using Realm.Persistance.Data.Helpers;

namespace Realm.Persistance.Data;

public sealed class Vehicle
{
#pragma warning disable CS8618
    public string Id { get; set; }
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
#pragma warning restore CS8618
}
