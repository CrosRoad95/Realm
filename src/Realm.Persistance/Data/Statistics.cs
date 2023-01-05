namespace Realm.Persistance.Data;

public class Statistics
{
    public Guid Id { get; set; }
    public float TraveledDistanceInVehicleAsDriver { get; set; }
    public float TraveledDistanceInVehicleAsPassager { get; set; }
    public float TraveledDistanceSwimming { get; set; }
    public float TraveledDistanceByFoot { get; set; }
    public float TraveledDistanceInAir { get; set; }

    public User User { get; set; }
}
