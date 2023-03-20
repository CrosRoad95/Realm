namespace Realm.Persistance.Data;

public sealed class JobUpgrade
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public short JobId { get; set; }
    public int UpgradeId { get; set; }
}
