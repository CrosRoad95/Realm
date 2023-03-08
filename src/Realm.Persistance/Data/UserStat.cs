namespace Realm.Persistance.Data;

public class UserStat
{
    public int UserId { get; set; }
    public int StatId { get; set; }
    public float Value { get; set; }

    public User? User { get; set; }
}
