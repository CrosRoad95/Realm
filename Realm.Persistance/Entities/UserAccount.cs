namespace Realm.Persistance.Entities;

public class UserAccount : IId
{
    public Guid Id { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
    public string RegisterIp { get; set; }
    public string RegisterSerial { get; set; }
    public string LastIp { get; set; }
    public string LastSerial { get; set; }
    public string PlayTime { get; set; }

    public List<AdminGroup> AdminGroups { get; set; } = new();
}
