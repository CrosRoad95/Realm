namespace RealmCore.Persistance.Data;

public sealed class DailyVisitsData
{
    public int UserId { get; set; }
    public DateTime LastVisit { get; set; } = DateTime.MinValue;
    public int VisitsInRow { get; set; }
    public int VisitsInRowRecord { get; set; }
}
