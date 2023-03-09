using Realm.Common.Providers;

namespace Realm.Domain.Components.Players;

[ComponentUsage(false)]
public class DailyVisitsCounterComponent : Component
{
    [Inject]
    private IDateTimeProvider DateTimeProvider { get; set; } = default!;
    public DateTime LastVisit { get; set; } = DateTime.MinValue;
    public int VisitsInRow { get; set; }
    public int VisitsInRowRecord { get; set; }

    public event Action<Entity, int, bool>? PlayerVisited;
    public event Action<Entity, int>? PlayerVisitsRecord;

    public DailyVisitsCounterComponent()
    {
    }

    internal DailyVisitsCounterComponent(DailyVisits dailyVisits)
    {
        LastVisit = dailyVisits.LastVisit;
        VisitsInRow = dailyVisits.VisitsInRow;
        VisitsInRowRecord = dailyVisits.VisitsInRowRecord;
    }

    protected override void Load()
    {
        Update();
    }

    private void Update()
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
            PlayerVisitsRecord?.Invoke(Entity, VisitsInRowRecord);
        }

        PlayerVisited?.Invoke(Entity, VisitsInRow, reseted);
        LastVisit = DateTimeProvider.Now;
    }
}

