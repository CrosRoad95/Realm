using Realm.Persistance.Data.Helpers;

namespace Realm.Persistance.Data;

public class VehicleAccess
{
    public int Id { get; set; }
    public string VehicleId { get; set; }
    public Guid UserId { get; set; }
    public VehicleAccessDescription Description { get; set; }

    public Vehicle Vehicle { get; set; }
    public User User { get; set; }
}
