namespace Realm.Domain.Concepts;

public struct VehicleAccess
{
    public int? Id { get; set; }
    public int? UserId { get; set; }
    public bool Ownership { get; set; }
}
