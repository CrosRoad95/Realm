namespace Realm.Persistance.Data;

public class UserUpgrade
{
    public int UserId { get; set; }
    public int UpgradeId { get; set; }

    public User? User { get; set; }
}
