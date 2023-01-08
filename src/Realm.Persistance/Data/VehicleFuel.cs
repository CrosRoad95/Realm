namespace Realm.Persistance.Data;

public class VehicleFuel
{
    public string VehicleId { get; set; }
    public string FuelType { get; set; }
    public float MinimumDistanceThreshold { get; set; }
    public float FuelConsumptionPerOneKm { get; set; }
    public float Amount { get; set; }
    public float MaxCapacity { get; set; }
    public bool Active { get; set; }

    public virtual Vehicle Vehicle { get; set; }
}
