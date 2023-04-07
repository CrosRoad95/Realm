namespace Realm.Persistance.Data;

public sealed class JobStatisticsData
{
    public int UserId { get; set; }
    public short JobId { get; set; }
    public ulong Points { get; set; }
    public ulong TimePlayed { get; set; }
    public DateOnly Date { get; set; }
}
