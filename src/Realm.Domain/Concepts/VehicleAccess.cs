namespace Realm.Domain.Concepts;

public class VehicleAccess
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public bool Ownership { get; set; }
}
