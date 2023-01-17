namespace Realm.Persistance.Data;

public class JobStatistics
{
    public Guid UserId { get; set; }
    public short JobId { get; set; }
    public ulong Points { get; set; }
    public ulong TimePlayed { get; set; }

    public virtual User User { get; set; }
}
