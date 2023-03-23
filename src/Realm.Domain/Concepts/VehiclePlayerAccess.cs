namespace Realm.Domain.Concepts;

public struct VehiclePlayerAccess
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public byte AccessType { get; set; }
    public string? CustomValue { get; set; }
}
