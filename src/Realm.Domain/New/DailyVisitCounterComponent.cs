namespace Realm.Domain.New;

[Serializable]
public class DailyVisitsCounterComponent : Component
{
    [ScriptMember("lastVisit")]
    public DateTime LastVisit { get; set; } = DateTime.MinValue;
    [ScriptMember("visitsInRow")]
    public int VisitsInRow { get; set; }

    public event Action<Player, int, bool>? PlayerVisited;
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

        var player = Entity.InternalGetRequiredComponent<PlayerElementCompoent>().Player;
        PlayerVisited?.Invoke(player, VisitsInRow, reseted);
        LastVisit = DateTime.Now;
    }
}

