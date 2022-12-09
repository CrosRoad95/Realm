namespace Realm.Persistance.Data;

public sealed class User : IdentityUser<Guid>
{
#pragma warning disable CS8618
    public string Nick { get; set; }
    public DateTime? RegisteredDateTime { get; set; }
    public DateTime? LastLogindDateTime { get; set; }
    public string? RegisterSerial { get; set; }
    public string? RegisterIp { get; set; }
    public string? LastSerial { get; set; }
    public string? LastIp { get; set; }
    public ulong PlayTime { get; set; }
    public short Skin { get; set; }
    public double Money { get; set; }
    public string? Components { get; set; }
    public string? Inventory { get; set; }
#pragma warning restore CS8618

    public ICollection<UserData>? PlayerData { get; set; }
    public ICollection<UserLicense>? Licenses { get; set; }
}
