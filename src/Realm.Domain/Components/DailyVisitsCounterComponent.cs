namespace Realm.Domain.Components;

[Serializable]
public class DailyVisitsCounterComponent : IElementComponent
{
    private RPGPlayer _player = default!;

    [ScriptMember("name")]
    public string Name => "DailyVisits";

    [ScriptMember("lastVisit")]
    public DateTime LastVisit { get; set; } = DateTime.MinValue;
    [ScriptMember("visitsInRow")]
    public int VisitsInRow { get; set; }

    public event Action<RPGPlayer, int, bool>? PlayerVisited;
    public DailyVisitsCounterComponent()
    {
    }

    public DailyVisitsCounterComponent(SerializationInfo info, StreamingContext context)
    {
        LastVisit = info.GetDateTime(nameof(LastVisit));
        VisitsInRow = info.GetInt32(nameof(VisitsInRow));
    }

    [NoScriptAccess]
    public void SetOwner(Element element)
    {
        if (_player != null)
            throw new Exception("Component already attached to element");
        if (element is not RPGPlayer rpgPlayer)
            throw new Exception("Not supported element type, expected: RPGPlayer");
        _player = rpgPlayer;
        _player.LoggedOut += LoggedOut;
        Update();
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
        PlayerVisited?.Invoke(_player, VisitsInRow, reseted);
        LastVisit = DateTime.Now;
    }

    private void LoggedOut(RPGPlayer player, string accountId)
    {
        _player.LoggedOut -= LoggedOut;
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(LastVisit), LastVisit);
        info.AddValue(nameof(VisitsInRow), VisitsInRow);
    }
}
