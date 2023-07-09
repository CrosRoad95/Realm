namespace RealmCore.Persistance.Data;

public sealed class VehicleUserAccessData
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public int UserId { get; set; }
    public byte AccessType { get; set; }
    public string? CustomValue { get; set; }

    public VehicleData? Vehicle { get; set; }
    public UserData? User { get; set; }
}
