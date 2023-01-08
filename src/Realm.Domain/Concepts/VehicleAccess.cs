namespace Realm.Domain.Concepts;

public struct VehicleAccess
{
    public Guid? Id { get; set; }
    public Guid? UserId { get; set; }
    public bool Ownership { get; set; }
}
