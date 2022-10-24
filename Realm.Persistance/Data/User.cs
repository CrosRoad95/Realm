namespace Realm.Persistance.Data;

public sealed class User : IdentityUser<Guid>
{
    public string Nick { get; set; }
    public DateTime? RegisteredDateTime { get; set; }
    public DateTime? LastLogindDateTime { get; set; }
    public string? RegisterSerial { get; set; }
    public string? RegisterIp { get; set; }
    public string? LastSerial { get; set; }
    public string? LastIp { get; set; }
    public ulong PlayTime { get; set; }
    public short Skin { get; set; }
}
