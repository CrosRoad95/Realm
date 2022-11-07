namespace Realm.Persistance.Data;

public sealed class PlayerData
{
    public Guid UserId { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }

    public User? User { get; set; }
}
