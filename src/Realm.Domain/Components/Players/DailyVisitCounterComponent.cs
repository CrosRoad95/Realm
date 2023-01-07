namespace Realm.Domain.Components.Players;

public class DailyVisitsCounterComponent : Component
{
    public DateTime LastVisit { get; set; } = DateTime.MinValue;
    public int VisitsInRow { get; set; }
    public int VisitsInRowRecord { get; set; }

    public event Action<Entity, int, bool>? PlayerVisited;
    public event Action<Entity, int>? PlayerVisitsRecord;

    public DailyVisitsCounterComponent(DailyVisits dailyVisits)
    {
        LastVisit = dailyVisits.LastVisit;
        VisitsInRow = dailyVisits.VisitsInRow;
        VisitsInRowRecord = dailyVisits.VisitsInRowRecord;
    }

    public DailyVisitsCounterComponent()
    {
    }

    public override Task Load()
    {
        Update();
        return Task.CompletedTask;
    }

    private void Update()
    {
        if (LastVisit.Date == DateTime.Now.Date)
            return;

        bool reseted = false;

        if (LastVisit.Date.AddDays(1) == DateTime.Now.Date)
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
        LastVisit = DateTime.Now;
    }
}

