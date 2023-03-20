namespace Realm.Persistance.Data;

public sealed class VehicleAccess
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public int UserId { get; set; }
    public VehicleAccessDescription Description { get; set; }

    public Vehicle? Vehicle { get; set; }
    public User? User { get; set; }
}
