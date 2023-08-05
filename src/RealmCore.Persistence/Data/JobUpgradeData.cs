namespace RealmCore.Persistence.Data;

public sealed class JobUpgradeData
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public short JobId { get; set; }
    public int UpgradeId { get; set; }
}
