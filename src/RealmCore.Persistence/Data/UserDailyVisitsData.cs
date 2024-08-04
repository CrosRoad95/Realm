namespace RealmCore.Persistence.Data;

public sealed class UserDailyVisitsData
{
    public int UserId { get; set; }
    public DateTime LastVisit { get; set; } = DateTime.MinValue;
    public int VisitsInRow { get; set; }
    public int VisitsInRowRecord { get; set; }
}
