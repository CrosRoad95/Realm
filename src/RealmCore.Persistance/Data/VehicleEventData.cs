namespace RealmCore.Persistence.Data;

public class VehicleEventData
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public int EventType { get; set; }
    public DateTime DateTime { get; set; }
}
