namespace Realm.Persistance.Data;

public class VehicleAccess
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid? UserId { get; set; }
    public VehicleAccessDescription Description { get; set; }

    public virtual Vehicle? Vehicle { get; set; }
    public virtual User? User { get; set; }
}
