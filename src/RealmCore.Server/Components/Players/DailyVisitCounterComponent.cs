using RealmCore.Persistence.Data;

namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class DailyVisitsCounterComponent : Component
{
    public DateTime LastVisit { get; set; } = DateTime.MinValue;
    public int VisitsInRow { get; set; }
    public int VisitsInRowRecord { get; set; }

    public event Action<DailyVisitsCounterComponent, int, bool>? PlayerVisited;
    public event Action<DailyVisitsCounterComponent, int>? PlayerVisitsRecord;

    public DailyVisitsCounterComponent()
    {
    }

    internal DailyVisitsCounterComponent(DailyVisitsData dailyVisits)
    {
        LastVisit = dailyVisits.LastVisit;
        VisitsInRow = dailyVisits.VisitsInRow;
        VisitsInRowRecord = dailyVisits.VisitsInRowRecord;
    }

    internal void Update(DateTime now)
    {
        var nowDate = now.Date;
        if (LastVisit.Date == nowDate)
            return;

        bool reset = false;

        if (LastVisit.Date.AddDays(1) == nowDate || LastVisit == DateTime.MinValue)
        {
            VisitsInRow++;
        }
        else
        {
            VisitsInRow = 0;
            reset = true;
        }

        if (VisitsInRow > VisitsInRowRecord) // Doesn't check if day passed because value can be arbitrarily changed
        {
            VisitsInRowRecord = VisitsInRow;
            PlayerVisitsRecord?.Invoke(this, VisitsInRowRecord);
        }

        PlayerVisited?.Invoke(this, VisitsInRow, reset);
        LastVisit = nowDate;
    }
}

