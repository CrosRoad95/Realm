namespace Realm.Persistance.Data;

public sealed class VehiclePlayerAccess
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public int UserId { get; set; }
    public byte AccessType { get; set; }
    public string? CustomValue { get; set; }

    public Vehicle? Vehicle { get; set; }
    public User? User { get; set; }
}
