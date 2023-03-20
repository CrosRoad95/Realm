namespace Realm.Persistance.Data;

public sealed class DailyVisits
{
    public int UserId { get; set; }
    public DateTime LastVisit { get; set; } = DateTime.MinValue;
    public int VisitsInRow { get; set; }
    public int VisitsInRowRecord { get; set; }
}
