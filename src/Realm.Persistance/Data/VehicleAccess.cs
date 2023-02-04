namespace Realm.Persistance.Data;

public class VehicleAccess
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public int UserId { get; set; }
    public VehicleAccessDescription Description { get; set; }

    public virtual Vehicle? Vehicle { get; set; }
    public virtual User? User { get; set; }
}
