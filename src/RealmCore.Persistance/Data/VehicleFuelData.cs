namespace RealmCore.Persistence.Data;

public sealed class VehicleFuelData
{
    public int VehicleId { get; set; }
    public short FuelType { get; set; }
    public float MinimumDistanceThreshold { get; set; }
    public float FuelConsumptionPerOneKm { get; set; }
    public float Amount { get; set; }
    public float MaxCapacity { get; set; }
    public bool Active { get; set; }
}
