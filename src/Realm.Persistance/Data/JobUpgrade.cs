namespace Realm.Persistance.Data;

public class JobUpgrade
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public short JobId { get; set; }
    public int UpgradeId { get; set; }

    public virtual User? User { get; set; }
}
