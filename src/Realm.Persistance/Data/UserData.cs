namespace Realm.Persistance.Data;

public sealed class UserData
{
#pragma warning disable CS8618
    public Guid UserId { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
#pragma warning restore CS8618

    public User? User { get; set; }
}
