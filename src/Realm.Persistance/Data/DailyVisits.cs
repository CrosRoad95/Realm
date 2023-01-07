namespace Realm.Persistance.Data;

public class DailyVisits
{
    public Guid UserId { get; set; }
    public DateTime LastVisit { get; set; } = DateTime.MinValue;
    public int VisitsInRow { get; set; }
    public int VisitsInRowRecord { get; set; }

    public User User { get; set; }
}
