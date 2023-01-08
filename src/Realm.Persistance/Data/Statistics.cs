namespace Realm.Persistance.Data;

public class Statistics
{
    public Guid UserId { get; set; }
    public float TraveledDistanceInVehicleAsDriver { get; set; }
    public float TraveledDistanceInVehicleAsPassager { get; set; }
    public float TraveledDistanceSwimming { get; set; }
    public float TraveledDistanceByFoot { get; set; }
    public float TraveledDistanceInAir { get; set; }

    public virtual User User { get; set; }
}
