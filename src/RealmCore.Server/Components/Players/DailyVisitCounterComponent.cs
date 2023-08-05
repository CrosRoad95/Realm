using RealmCore.Persistence.Data;

namespace RealmCore.Server.Components.Players;

[ComponentUsage(false)]
public class DailyVisitsCounterComponent : Component
{
    [Inject]
    private IDateTimeProvider DateTimeProvider { get; set; } = default!;
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

    protected override void Load()
    {
        Update();
    }

    internal void Update()
    {
        if (LastVisit.Date == DateTimeProvider.Now.Date)
            return;

        bool reseted = false;

        if (LastVisit.Date.AddDays(1) == DateTimeProvider.Now.Date)
        {
            VisitsInRow++;
        }
        else
        {
            VisitsInRow = 0;
            reseted = true;
        }

        if (VisitsInRow > VisitsInRowRecord) // Doesn't check if day passed because value can be arbitrarily changed
        {
            VisitsInRowRecord = VisitsInRow;
            PlayerVisitsRecord?.Invoke(this, VisitsInRowRecord);
        }

        PlayerVisited?.Invoke(this, VisitsInRow, reseted);
        LastVisit = DateTimeProvider.Now;
    }
}

