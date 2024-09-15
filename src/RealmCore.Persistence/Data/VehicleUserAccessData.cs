namespace RealmCore.Persistence.Data;

public abstract class VehicleAccessDataBase
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public int AccessType { get; set; }
    public string? Metadata { get; set; }

    public VehicleData? Vehicle { get; set; }
}

public sealed class VehicleUserAccessData : VehicleAccessDataBase
{
    public int UserId { get; set; }
    public UserData? User { get; set; }
}

public sealed class VehicleGroupAccessData : VehicleAccessDataBase
{
    public int GroupId { get; set; }
    public GroupData? Group { get; set; }
}
