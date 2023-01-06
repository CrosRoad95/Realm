namespace Realm.Persistance.Data;

public sealed class User : IdentityUser<Guid>
{
#pragma warning disable CS8618
    public string? Nick { get; set; }
    public DateTime? RegisteredDateTime { get; set; }
    public DateTime? LastLogindDateTime { get; set; }
    public string? RegisterSerial { get; set; }
    public string? RegisterIp { get; set; }
    public string? LastSerial { get; set; }
    public string? LastIp { get; set; }
    public ulong PlayTime { get; set; }
    public short Skin { get; set; }
    public decimal Money { get; set; }

    public TransformAndMotion? LastTransformAndMotion { get; set; } = null;
#pragma warning restore CS8618

    public ICollection<UserLicense> Licenses { get; set; } = new List<UserLicense>();
    public ICollection<VehicleAccess> VehicleAccesses { get; set; } = new List<VehicleAccess>();
    public Inventory? Inventory { get; set; }
    public DailyVisits? DailyVisits { get; set; }
    public Statistics? Statistics { get; set; }
    public ICollection<Achievement> Achievements { get; set; } = new List<Achievement>();
}
